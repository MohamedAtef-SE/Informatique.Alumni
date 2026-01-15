
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.Guidance;

[Authorize]
public class AdvisingAppService : AlumniAppService
{
    private readonly IRepository<AdvisingRequest, Guid> _repository;
    private readonly IRepository<AlumniProfile, Guid> _alumniProfileRepository;
    private readonly ILocalEventBus _localEventBus;
    private readonly AlumniApplicationMappers _alumniMappers;

    public AdvisingAppService(
        IRepository<AdvisingRequest, Guid> repository,
        IRepository<AlumniProfile, Guid> alumniProfileRepository,
        ILocalEventBus localEventBus,
        AlumniApplicationMappers alumniMappers)
    {
        _repository = repository;
        _alumniProfileRepository = alumniProfileRepository;
        _localEventBus = localEventBus;
        _alumniMappers = alumniMappers;
    }

    [Authorize(AlumniPermissions.Guidance.ManageRequests)]
    public async Task<PagedResultDto<AdvisingRequestDto>> GetListAsync(AdvisingRequestFilterDto input)
    {
        var currentBranchId = await GetCurrentUserBranchIdAsync();

        // 1. Branch Security Enforcement
        if (!await AuthorizationService.IsGrantedAsync(AlumniPermissions.Guidance.ViewAllBranches))
        {
            input.BranchId = currentBranchId;
        }

        // 2. Query Construction
        var query = await _repository.GetQueryableAsync();
        var alumniQuery = await _alumniProfileRepository.GetQueryableAsync();

        // Join to filter by Branch if needed
        var combinedQuery = from req in query
                            join al in alumniQuery on req.AlumniId equals al.Id
                            select new { Req = req, Al = al };

        if (input.BranchId.HasValue)
        {
            combinedQuery = combinedQuery.Where(x => x.Al.BranchId == input.BranchId.Value);
        }

        // Date & Status Filters
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

    [Authorize(AlumniPermissions.Guidance.ManageRequests)]
    public async Task UpdateStatusAsync(Guid id, UpdateAdvisingStatusDto input)
    {
        var request = await _repository.GetAsync(id);

        // 1. Branch Security
        if (!await AuthorizationService.IsGrantedAsync(AlumniPermissions.Guidance.ViewAllBranches))
        {
            var currentBranchId = await GetCurrentUserBranchIdAsync();
            var alumni = await _alumniProfileRepository.GetAsync(request.AlumniId);
            if (alumni.BranchId != currentBranchId)
            {
                throw new UserFriendlyException("You do not have permission to manage requests for this branch.");
            }
        }

        // 2. Immutability Logic
        if (request.Status != AdvisingRequestStatus.Requested) // Assuming Requested is Pending
        {
            throw new UserFriendlyException($"Cannot change status of a finalized request. Current Status: {request.Status}");
        }

        var oldStatus = request.Status;
        
        // 3. Update Logic
        request.Status = input.Status;
        request.AdminNotes = input.Notes;
        
        await _repository.UpdateAsync(request);

        // 4. Notification Trigger
        await _localEventBus.PublishAsync(new AdvisingRequestStatusChangedEto
        {
            RequestId = request.Id,
            AlumniId = request.AlumniId,
            OldStatus = oldStatus,
            NewStatus = input.Status
        });
    }

    private async Task<Guid> GetCurrentUserBranchIdAsync()
    {
        var claim = CurrentUser.FindClaim("BranchId");
        if (claim == null || !Guid.TryParse(claim.Value, out var guid))
        {
             // Fallback: Check Profile if mostly Employee also has a Profile (unlikely for strict separation but possible)
             // Or throw exception
             throw new UserFriendlyException("Current user does not have an assigned branch.");
        }
        return await Task.FromResult(guid);
    }
}
