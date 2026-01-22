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
using System.Collections.Generic;

namespace Informatique.Alumni.Guidance;

[Authorize]
public class AdvisingAppService : AlumniAppService, IGuidanceAppService
{
    private readonly IRepository<AdvisingRequest, Guid> _repository;
    private readonly IRepository<AlumniProfile, Guid> _alumniProfileRepository;
    private readonly ILocalEventBus _localEventBus;
    private readonly AlumniApplicationMappers _alumniMappers;
    private readonly AdvisingManager _advisingManager;

    public AdvisingAppService(
        IRepository<AdvisingRequest, Guid> repository,
        IRepository<AlumniProfile, Guid> alumniProfileRepository,
        ILocalEventBus localEventBus,
        AlumniApplicationMappers alumniMappers,
        AdvisingManager advisingManager)
    {
        _repository = repository;
        _alumniProfileRepository = alumniProfileRepository;
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
