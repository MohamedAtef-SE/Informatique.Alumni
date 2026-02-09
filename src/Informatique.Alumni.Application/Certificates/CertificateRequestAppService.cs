using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Certificates;

// [Authorize(AlumniPermissions.Certificates.Default)] // Removed class-level auth
public class CertificateRequestAppService : AlumniAppService, ICertificateRequestAppService
{
    private readonly IRepository<CertificateRequest, Guid> _repository;
    private readonly IRepository<CertificateDefinition, Guid> _definitionRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IRepository<AlumniProfile, Guid> _alumniProfileRepository;
    private readonly CertificateManager _certificateManager;
    private readonly AlumniApplicationMappers _alumniMappers;

    public CertificateRequestAppService(
        IRepository<CertificateRequest, Guid> repository,
        IRepository<CertificateDefinition, Guid> definitionRepository,
        IRepository<Branch, Guid> branchRepository,
        IRepository<Education, Guid> educationRepository,
        IRepository<AlumniProfile, Guid> alumniProfileRepository,
        CertificateManager certificateManager,
        AlumniApplicationMappers alumniMappers)
    {
        _repository = repository;
        _definitionRepository = definitionRepository;
        _branchRepository = branchRepository;
        _educationRepository = educationRepository;
        _alumniProfileRepository = alumniProfileRepository;
        _certificateManager = certificateManager;
        _alumniMappers = alumniMappers;
    }

    [Authorize]
    public async Task<CertificateRequestDto> GetAsync(Guid id)
    {
        var queryable = await _repository.WithDetailsAsync(x => x.Items);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable, x => x.Id == id);
        
        if (entity == null)
        {
             throw new EntityNotFoundException(typeof(CertificateRequest), id);
        }

        var dto = _alumniMappers.MapToDto(entity);
        
        // Populate item names
        await PopulateItemNamesAsync(dto);
        
        // Populate branch name if applicable
        if (entity.TargetBranchId.HasValue)
        {
            var branch = await _branchRepository.GetAsync(entity.TargetBranchId.Value);
            dto.TargetBranchName = branch.Name;
        }
        
        return dto;
    }



    /// <summary>
    /// Employee Follow-Up: Advanced search with branch-based security filtering.
    /// Business Rule: BranchAdmin can only see their branch, SuperAdmin can see all.
    /// </summary>
    [Authorize(AlumniPermissions.Certificates.Default)]
    public async Task<PagedResultDto<CertificateRequestDto>> GetListAsync(CertificateRequestFilterDto input)
    {
        // ========== BUSINESS RULE: Branch Security ==========
        // Get current user's branch (assuming it's stored as CollegeId claim)
        // If user has no CollegeId (null), they're a SuperAdmin and can see all branches
        var currentUserBranchId = CurrentUser.GetCollegeId(); // Returns null for SuperAdmin

        Guid? effectiveBranchId = input.BranchId;
        
        if (currentUserBranchId.HasValue)
        {
            // User is a Branch Admin - force them to only see their branch (ignore UI input)
            effectiveBranchId = currentUserBranchId;
        }
        // If currentUserBranchId is null (SuperAdmin), allow filtering by any branch (use input.BranchId as-is)

        // ========== Build Query with Filtering ==========
        var queryable = await _repository.WithDetailsAsync(x => x.Items);

        // Branch filter (security-enforced)
        if (effectiveBranchId.HasValue)
        {
            queryable = queryable.Where(x => x.TargetBranchId == effectiveBranchId.Value);
        }


        if (input.GraduationYear.HasValue)
        {
            // Business Rule: Filter requests by Alumni's graduation year
            // Optimization: Get relevant AlumniIds first
            // Education -> AlumniProfile -> UserId (AlumniId)
            var educations = await _educationRepository.GetListAsync(x => x.GraduationYear == input.GraduationYear.Value);
            var profileIds = educations.Select(x => x.AlumniProfileId).Distinct().ToList();
            
            if (profileIds.Any())
            {
                var profiles = await _alumniProfileRepository.GetListAsync(x => profileIds.Contains(x.Id));
                var distinctAlumniIds = profiles.Select(x => x.UserId).Distinct().ToList();
                queryable = queryable.Where(x => distinctAlumniIds.Contains(x.AlumniId));
            }
            else
            {
                 // No matches found
                 queryable = queryable.Where(x => false);
            }
        }

        // Delivery Method filter (Office vs Home)
        if (input.DeliveryMethod.HasValue)
        {
            queryable = queryable.Where(x => x.DeliveryMethod == input.DeliveryMethod.Value);
        }

        // Status filter
        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        // Alumni ID filter
        if (input.AlumniId.HasValue)
        {
            queryable = queryable.Where(x => x.AlumniId == input.AlumniId.Value);
        }

        // Date Range filter
        if (input.FromDate.HasValue)
        {
            queryable = queryable.Where(x => x.CreationTime >= input.FromDate.Value);
        }

        if (input.ToDate.HasValue)
        {
            queryable = queryable.Where(x => x.CreationTime <= input.ToDate.Value);
        }

        // Get total count before paging
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply sorting and paging
        var sorting = !string.IsNullOrWhiteSpace(input.Sorting) ? input.Sorting : "CreationTime DESC";
        queryable = ApplySorting(queryable, sorting);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

        // Execute query
        var entities = await AsyncExecuter.ToListAsync(queryable);
        // Map to DTOs
        var dtos = _alumniMappers.MapToDtos(entities);

        // Pre-fetch Branch Names for optimization
        // Pre-fetch Branch Names for optimization
        var branchIds = dtos.Where(x => x.TargetBranchId.HasValue).Select(x => x.TargetBranchId.Value).Distinct().ToList();
        var branches = await _branchRepository.GetListAsync(x => branchIds.Contains(x.Id));
        var branchDict = branches.ToDictionary(k => k.Id, v => v.Name);

        // ========== Bulk Fetch for Items Implementation (N+1 Fix) ==========
        var allItems = dtos.SelectMany(d => d.Items).ToList();
        
        // 1. Certificate Definitions
        var definitionIds = allItems.Select(x => x.CertificateDefinitionId).Distinct().ToList();
        var definitions = await _definitionRepository.GetListAsync(x => definitionIds.Contains(x.Id));
        var defDict = definitions.ToDictionary(x => x.Id, x => x.NameEn);

        // 2. Qualifications
        var qualificationIds = allItems.Where(x => x.QualificationId.HasValue)
                                       .Select(x => x.QualificationId!.Value).Distinct().ToList();
        Dictionary<Guid, string> qualDict = new();
        if (qualificationIds.Any())
        {
            var qualifications = await _educationRepository.GetListAsync(x => qualificationIds.Contains(x.Id));
            qualDict = qualifications.ToDictionary(x => x.Id, x => $"{x.Degree} - {x.InstitutionName}");
        }

        // 3. Map in Memory
        foreach (var dto in dtos)
        {
            if (dto.TargetBranchId.HasValue && branchDict.TryGetValue(dto.TargetBranchId.Value, out var branchName))
            {
                dto.TargetBranchName = branchName;
            }

            foreach (var item in dto.Items)
            {
                if (defDict.TryGetValue(item.CertificateDefinitionId, out var defName))
                {
                    item.CertificateDefinitionName = defName;
                }
                
                if (item.QualificationId.HasValue && qualDict.TryGetValue(item.QualificationId.Value, out var qualName))
                {
                    item.QualificationName = qualName;
                }
            }
        }

        return new PagedResultDto<CertificateRequestDto>(totalCount, dtos);
    }

    [Authorize]
    public async Task<PagedResultDto<CertificateRequestDto>> GetMyRequestsAsync(PagedAndSortedResultRequestDto input)
    {
        var filter = new CertificateRequestFilterDto
        {
            SkipCount = input.SkipCount,
            MaxResultCount = input.MaxResultCount,
            Sorting = input.Sorting,
            AlumniId = CurrentUser.GetId()
        };
        return await GetListAsync(filter);
    }

    private static IQueryable<CertificateRequest> ApplySorting(IQueryable<CertificateRequest> query, string sorting)
    {
        // Simple sorting implementation - enhance as needed
        if (sorting.Contains("CreationTime", StringComparison.OrdinalIgnoreCase))
        {
            return sorting.Contains("DESC", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.CreationTime)
                : query.OrderBy(x => x.CreationTime);
        }

        if (sorting.Contains("Status", StringComparison.OrdinalIgnoreCase))
        {
            return sorting.Contains("DESC", StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(x => x.Status)
                : query.OrderBy(x => x.Status);
        }

        // Default sorting
        return query.OrderByDescending(x => x.CreationTime);
    }

    [Authorize] // Changed from specific permission to general auth to ensure access
    public async Task<CertificateRequestDto> CreateAsync(CreateCertificateRequestDto input)
    {
        try
        {
            // Validate input
            if (input.Items == null || !input.Items.Any())
            {
                throw new UserFriendlyException("At least one certificate item is required.");
            }

            // Map DTOs to domain inputs
            var itemInputs = input.Items.Select(x => new CreateItemInput
            {
                CertificateDefinitionId = x.CertificateDefinitionId,
                QualificationId = x.QualificationId,
                Language = x.Language
            }).ToList();

            // Use domain service to create request with ALL business rules enforced
            var request = await _certificateManager.CreateRequestAsync(
                GuidGenerator.Create(),
                CurrentUser.GetId(),
                itemInputs,
                input.DeliveryMethod,
                input.TargetBranchId,
                input.DeliveryAddress,
                input.UserNotes
            );

            await _repository.InsertAsync(request);

            // Optimization: Map directly to avoid re-fetching entity
            var dto = _alumniMappers.MapToDto(request);
            await PopulateItemNamesAsync(dto);

            if (request.TargetBranchId.HasValue)
            {
                var branch = await _branchRepository.GetAsync(request.TargetBranchId.Value);
                dto.TargetBranchName = branch.Name;
            }
            
            return dto;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateAsync Failed: {ex.Message} {ex.StackTrace}");
            if (ex is Volo.Abp.BusinessException bex)
            {
                 // Pass business exception details
                 throw new UserFriendlyException($"Business Rule Failed: {bex.Message} Code: {bex.Code}");
            }
            throw new UserFriendlyException($"Internal Error: {ex.Message}");
        }
    }

    [Authorize(AlumniPermissions.Certificates.Process)]
    public async Task<CertificateRequestDto> ProcessAsync(Guid id, ProcessCertificateRequestDto input)
    {
        var entity = await _repository.GetAsync(id);

        if (input.Status == CertificateRequestStatus.Rejected)
        {
            entity.Reject(input.AdminNotes ?? "No reason provided.");
        }
        else if (input.Status == CertificateRequestStatus.Processing)
        {
            entity.MoveToProcessing();
        }
        else if (input.Status == CertificateRequestStatus.ReadyForPickup)
        {
            entity.MarkAsReadyForPickup();
            
            // Generate verification hashes for all items
            foreach (var item in entity.Items)
            {
                _certificateManager.MarkItemAsReady(item);
            }
        }
        else if (input.Status == CertificateRequestStatus.OutForDelivery)
        {
            entity.MarkAsOutForDelivery();
            
            // Generate verification hashes for all items
            foreach (var item in entity.Items)
            {
                _certificateManager.MarkItemAsReady(item);
            }
        }
        else if (input.Status == CertificateRequestStatus.Delivered)
        {
            entity.Deliver();
        }

        if (!string.IsNullOrWhiteSpace(input.AdminNotes))
        {
            entity.AddAdminNotes(input.AdminNotes);
        }

        await _repository.UpdateAsync(entity);
        return await GetAsync(entity.Id);
    }

    [Authorize(AlumniPermissions.Certificates.Request)]
    public async Task<CertificateRequestDto> RecordGatewayPaymentAsync(Guid id, RecordPaymentDto input)
    {
        var entity = await _repository.GetAsync(id);
        
        // Security: Ensure user owns this request
        if (entity.AlumniId != CurrentUser.GetId())
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.Unauthorized)
                .WithData("RequestId", id)
                .WithData("UserId", CurrentUser.GetId());
        }

        entity.RecordGatewayPayment(input.Amount);
        
        await _repository.UpdateAsync(entity);
        return await GetAsync(entity.Id);
    }

    /// <summary>
    /// Employee Follow-Up: Update certificate request status with audit trail.
    /// Business Rule: Uses ChangeStatus domain method to enforce "Two Paths" logic.
    /// </summary>
    [Authorize(AlumniPermissions.Certificates.Process)]
    public async Task<CertificateRequestDto> UpdateStatusAsync(Guid id, UpdateCertificateStatusDto input)
    {
        var entity = await _repository.GetAsync(id);
        
        // Use domain method to change status (enforces Two Paths rule and creates history)
        entity.ChangeStatus(
            input.NewStatus,
            CurrentUser.GetId(),
            GuidGenerator.Create,
            input.Note
        );
        
        // Generate verification hashes for items when marking ready
        if (input.NewStatus == CertificateRequestStatus.ReadyForPickup ||
            input.NewStatus == CertificateRequestStatus.OutForDelivery)
        {
            foreach (var item in entity.Items)
            {
                if (string.IsNullOrEmpty(item.VerificationHash))
                {
                    _certificateManager.MarkItemAsReady(item);
                }
            }
        }

        await _repository.UpdateAsync(entity);
        return await GetAsync(entity.Id);
    }

    [AllowAnonymous]
    public async Task<CertificateRequestDto> GetByHashAsync(string hash)
    {
        // Find the request by searching through items
        // Efficiently query the item by hash using Linq-to-Entities
        // This requires Items to be indexed or accessible
        var entity = await _repository.FirstOrDefaultAsync(r => r.Items.Any(i => i.VerificationHash == hash));
        
        if (entity == null)
        {
            throw new UserFriendlyException("Invalid or expired certificate hash.");
        }

        return await GetAsync(entity.Id);
    }

    private async Task PopulateItemNamesAsync(CertificateRequestDto dto)
    {
        var definitionIds = dto.Items.Select(x => x.CertificateDefinitionId).Distinct().ToList();
        var definitions = await _definitionRepository.GetListAsync(x => definitionIds.Contains(x.Id));
        var defDict = definitions.ToDictionary(x => x.Id, x => x.NameEn);
        
        var qualificationIds = dto.Items.Where(x => x.QualificationId.HasValue)
            .Select(x => x.QualificationId!.Value).Distinct().ToList();
        
        Dictionary<Guid, string> qualDict = new();
        if (qualificationIds.Any())
        {
            var qualifications = await _educationRepository.GetListAsync(x => qualificationIds.Contains(x.Id));
            qualDict = qualifications.ToDictionary(x => x.Id, x => $"{x.Degree} - {x.InstitutionName}");
        }
        
        foreach (var item in dto.Items)
        {
            if (defDict.TryGetValue(item.CertificateDefinitionId, out var defName))
            {
                item.CertificateDefinitionName = defName;
            }
            
            if (item.QualificationId.HasValue && qualDict.TryGetValue(item.QualificationId.Value, out var qualName))
            {
                item.QualificationName = qualName;
            }
        }
    }
}
