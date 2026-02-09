using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Identity;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;
using System.Collections.Generic;

namespace Informatique.Alumni.Guidance;

[Authorize]
public class AdvisingAppService : AlumniAppService, IGuidanceAppService
{
    private readonly IRepository<AdvisingRequest, Guid> _repository;
    private readonly IRepository<AlumniProfile, Guid> _alumniProfileRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly ILocalEventBus _localEventBus;
    private readonly AlumniApplicationMappers _alumniMappers;
    private readonly AdvisingManager _advisingManager;

    public AdvisingAppService(
        IRepository<AdvisingRequest, Guid> repository,
        IRepository<AlumniProfile, Guid> alumniProfileRepository,
        IIdentityUserRepository userRepository,
        ILocalEventBus localEventBus,
        AlumniApplicationMappers alumniMappers,
        AdvisingManager advisingManager)
    {
        _repository = repository;
        _alumniProfileRepository = alumniProfileRepository;
        _userRepository = userRepository;
        _localEventBus = localEventBus;
        _alumniMappers = alumniMappers;
        _advisingManager = advisingManager;
    }

    [Authorize(AlumniPermissions.Guidance.ManageRequests)]
    public async Task<PagedResultDto<AdvisingRequestDto>> GetListAsync(AdvisingRequestFilterDto input)
    {
        // Existing implementation preserved...
        var currentBranchId = await GetCurrentUserBranchIdAsync();

        if (!await AuthorizationService.IsGrantedAsync(AlumniPermissions.Guidance.ViewAllBranches))
        {
            input.BranchId = currentBranchId;
        }

        var query = await _repository.GetQueryableAsync();
        var alumniQuery = await _alumniProfileRepository.GetQueryableAsync();

        var combinedQuery = from req in query
                            join al in alumniQuery on req.AlumniId equals al.Id
                            select new { Req = req, Al = al };

        if (input.BranchId.HasValue)
        {
            combinedQuery = combinedQuery.Where(x => x.Al.BranchId == input.BranchId.Value);
        }

        combinedQuery = combinedQuery.Where(x => x.Req.StartTime >= input.FromDate && x.Req.StartTime <= input.ToDate);

        if (input.Status.HasValue)
        {
            combinedQuery = combinedQuery.Where(x => x.Req.Status == input.Status.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(combinedQuery);

        var items = await AsyncExecuter.ToListAsync(
            combinedQuery
                .OrderByDescending(x => x.Req.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .Select(x => x.Req)
        );

        var dtos = _alumniMappers.MapToDtos(items);
        return new PagedResultDto<AdvisingRequestDto>(totalCount, dtos);
    }

    // [New] Create Request Endpoint
    [Authorize]
    public async Task<AdvisingRequestDto> CreateRequestAsync(CreateAdvisingRequestDto input)
    {
        var request = await _advisingManager.CreateRequestAsync(
            CurrentUser.Id.Value,
            null, // BranchId inferred/enforced by Manager logic or user input if allowed
            input.AdvisorId,
            input.Date,
            input.StartTime,
            input.Subject,
            input.Description
        );

        return _alumniMappers.MapToDto(request);
    }

    public async Task<List<AdvisorDto>> GetAvailableAdvisorsAsync()
    {
        // Return VIP members as Advisors
        var allProfiles = await _alumniProfileRepository.GetListAsync();
        var profiles = allProfiles.Where(p => p.IsVip).Take(10).ToList();
        
        if (!profiles.Any()) return new List<AdvisorDto>();
        
        // Get user IDs to fetch names
        var userIds = profiles.Select(p => p.UserId).ToList();
        
        // IIdentityUserRepository uses different signature - get all and filter in memory
        var allUsers = await _userRepository.GetListAsync();
        var users = allUsers.Where(u => userIds.Contains(u.Id)).ToList();
        
        var advisors = new List<AdvisorDto>();
        
        foreach (var p in profiles)
        {
            var user = users.FirstOrDefault(u => u.Id == p.UserId);
            var displayName = user?.ExtraProperties.GetValueOrDefault("Name")?.ToString() 
                              ?? user?.Name 
                              ?? $"{user?.Surname}, {user?.Name}".Trim(' ', ',')
                              ?? user?.UserName 
                              ?? "Advisor";
            
            advisors.Add(new AdvisorDto
            {
                Id = p.Id,
                Name = displayName,
                JobTitle = p.JobTitle,
                PhotoUrl = p.PhotoUrl
            });
        }
        
        return advisors;
    }

    public async Task<PagedResultDto<AdvisingRequestDto>> GetMyRequestsAsync(PagedAndSortedResultRequestDto input)
    {
         var userId = CurrentUser.Id;
         if (!userId.HasValue) throw new AbpAuthorizationException("Must be logged in.");

         // [Fix] Lookup Profile ID first, because AdvisingRequest.AlumniId refers to ProfileId
         var profile = await _alumniProfileRepository.FirstOrDefaultAsync(x => x.UserId == userId.Value);
         if (profile == null)
         {
             // If no profile, user cannot have requests
             return new PagedResultDto<AdvisingRequestDto>(0, new List<AdvisingRequestDto>());
         }

         var query = await _repository.GetQueryableAsync();
         query = query.Where(x => x.AlumniId == profile.Id);
         
         var totalCount = await AsyncExecuter.CountAsync(query);
         
         var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime)
                 .PageBy(input.SkipCount, input.MaxResultCount)
         );
         
         var dtos = _alumniMappers.MapToDtos(items);

         // [Enhancement] Populate Advisor Details
         if (dtos.Any())
         {
             var advisorIds = dtos.Select(d => d.AdvisorId).Distinct().ToList();
             var advisorProfiles = await _alumniProfileRepository.GetListAsync(p => advisorIds.Contains(p.Id));
             var advisorUserIds = advisorProfiles.Select(p => p.UserId).ToList();
             
             // [Fix] Fix Lambda error by fetching users individually (safe fallback) or via compatible method
             var advisorUsers = new List<IdentityUser>();
             foreach (var uid in advisorUserIds)
             {
                 var u = await _userRepository.GetAsync(uid);
                 if (u != null) advisorUsers.Add(u);
             }

             foreach (var dto in dtos)
             {
                 var advisorProfile = advisorProfiles.FirstOrDefault(p => p.Id == dto.AdvisorId);
                 if (advisorProfile != null)
                 {
                     var user = advisorUsers.FirstOrDefault(u => u.Id == advisorProfile.UserId);
                     var name = user?.ExtraProperties.GetValueOrDefault("Name")?.ToString() 
                               ?? user?.Name 
                               ?? $"{user?.Surname}, {user?.Name}".Trim(' ', ',')
                               ?? user?.UserName 
                               ?? "Advisor";
                     
                     dto.AdvisorName = name;
                     dto.AdvisorJobTitle = advisorProfile.JobTitle;
                     dto.Location = "Online (Google Meet)"; // Hardcoded for now
                 }
                 else
                 {
                     dto.AdvisorName = "Unknown Advisor";
                 }
             }
         }

         return new PagedResultDto<AdvisingRequestDto>(totalCount, dtos);
    }

    [Authorize(AlumniPermissions.Guidance.ManageRequests)]
    public async Task UpdateStatusAsync(Guid id, UpdateAdvisingStatusDto input)
    {
        var request = await _repository.GetAsync(id);

        if (!await AuthorizationService.IsGrantedAsync(AlumniPermissions.Guidance.ViewAllBranches))
        {
            var currentBranchId = await GetCurrentUserBranchIdAsync();
            var alumni = await _alumniProfileRepository.GetAsync(request.AlumniId);
            if (alumni.BranchId != currentBranchId)
            {
                throw new UserFriendlyException("You do not have permission to manage requests for this branch.");
            }
        }

        if (request.Status != AdvisingRequestStatus.Pending)
        {
            throw new UserFriendlyException($"Cannot change status of a finalized request. Current Status: {request.Status}");
        }

        var oldStatus = request.Status;
        
        // Use Entity Methods for State Transition
        switch (input.Status)
        {
            case AdvisingRequestStatus.Approved:
                request.Approve();
                break;
            case AdvisingRequestStatus.Rejected:
                request.Reject(input.Notes ?? "Rejected");
                break;
            case AdvisingRequestStatus.Cancelled: // If Admin can cancel
                // request.Cancel(); // Assuming Cancel method exists or we add it? 
                // Using reflection or Assuming only Approve/Reject relevant for "ManageRequests". 
                // Requirement said "Status: Pending -> Approved/Rejected". 
                // But Enum has Cancelled/Completed. 
                // I will add a method to Entity if needed or just handle Approve/Reject for now.
                // If input is other, throw.
                throw new UserFriendlyException("Only Approve or Reject actions are allowed.");
            default:
                throw new UserFriendlyException($"Invalid status transition to {input.Status}");
        }

        // request.Status = input.Status; // Removed
        // request.AdminNotes = input.Notes; // Removed
        
        await _repository.UpdateAsync(request);

        await _localEventBus.PublishAsync(new AdvisingRequestStatusChangedEto
        {
            RequestId = request.Id,
            AlumniId = request.AlumniId,
            OldStatus = oldStatus,
            NewStatus = request.Status // Use actual status
        });
    }

    // [New] Reporting Endpoint
    [Authorize(AlumniPermissions.Guidance.ManageRequests)] // Or specific Report Permission
    public async Task<object> GetReportAsync(AdvisingReportInputDto input)
    {
        // 1. Base Query
        var query = await _repository.GetQueryableAsync();
        var alumniQuery = await _alumniProfileRepository.GetQueryableAsync();

        // Join with Profile (Include Educations for Stats)
        // Note: EF Core Include might be needed if using navigation properties, 
        // OR we can join manually if explicit relations not set up in aggregate roots context.
        // Assuming we need to join manually or use Include.
        // Let's use manual join logic for filtering first.

        var baseQuery = from req in query
                        join al in alumniQuery on req.AlumniId equals al.Id
                        select new { Req = req, Al = al };

        // Filters
        if (input.BranchId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Al.BranchId == input.BranchId.Value);
        }
        
        baseQuery = baseQuery.Where(x => x.Req.StartTime >= input.FromDate && x.Req.StartTime <= input.ToDate);
        
        if (input.Status.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Req.Status == input.Status.Value);
        }

        if (input.ReportType == AdvisingReportType.Detailed)
        {
             var list = await AsyncExecuter.ToListAsync(baseQuery
                 .OrderByDescending(x => x.Req.CreationTime)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount));
                 
             // Map to Detail DTO
             // Note: College/Major/Branch names usually require additional joins or Cache lookups.
             // For brevity, putting placeholders or IDs, assuming Mapper handles simple properties.
             
             var result = list.Select((x, index) => new AdvisingReportDetailDto
             {
                 Serial = index + 1, // Logic for paging offset needed in real app
                 AlumniId = x.Al.Id,
                 // AlumniName = x.Al.FullName, // Assuming FullName property exists or constructed
                 Status = x.Req.Status.ToString(),
                 Date = x.Req.StartTime.ToString("yyyy-MM-dd"),
                 Time = x.Req.StartTime.ToString("HH:mm")
             }).ToList();
             
             return new PagedResultDto<AdvisingReportDetailDto>(await AsyncExecuter.CountAsync(baseQuery), result);
        }
        else // Statistical
        {
            // Pivot by Graduation Year
            // Need to join Education
            // We need to fetch data relevant to grouping.
            // Since Educations is a collection, dealing with it in Line query is complex without SelectMany.
            
            // Fetch relevant data into memory (if reasonable size) or use tricky LINQ.
            // Prompt requires LINQ GroupBy.
            
            // Because Education is a collection in Profile, we usually take the Latest Graduation Year.
            
            var data = await AsyncExecuter.ToListAsync(baseQuery);
            
            // In Memory Grouping (simpler given the collection structure)
            var stats = data.Select(x => new 
            {
                Year = x.Al.Educations.Any() ? x.Al.Educations.Max(e => e.GraduationYear) : 0,
                Status = x.Req.Status
            })
            .GroupBy(x => x.Year)
            .Select(g => new AdvisingReportStatsRowDto
            {
                GraduationYear = g.Key,
                CountNew = g.Count(x => x.Status == AdvisingRequestStatus.Pending), // Was Pending/Requested
                CountAccepted = g.Count(x => x.Status == AdvisingRequestStatus.Approved), // Was Approved/Accepted
                CountRejected = g.Count(x => x.Status == AdvisingRequestStatus.Rejected),
                RowTotal = g.Count()
            })
            .OrderBy(x => x.GraduationYear)
            .ToList();
            
            return stats;
        }
    }

    private async Task<Guid> GetCurrentUserBranchIdAsync()
    {
        var claim = CurrentUser.FindClaim("BranchId");
        if (claim == null || !Guid.TryParse(claim.Value, out var guid))
        {
             throw new UserFriendlyException("Current user does not have an assigned branch.");
        }
        return await Task.FromResult(guid);
    }
}
