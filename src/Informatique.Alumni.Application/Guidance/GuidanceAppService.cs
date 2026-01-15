using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Users;
using Informatique.Alumni.Profiles;

namespace Informatique.Alumni.Guidance;

[Authorize]
public class GuidanceAppService : AlumniAppService, IGuidanceAppService
{
    private readonly IRepository<GuidanceSessionRule, Guid> _ruleRepository;
    private readonly IRepository<AdvisingRequest, Guid> _requestRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly GuidanceManager _guidanceManager;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly AlumniApplicationMappers _alumniMappers;

    public GuidanceAppService(
        IRepository<GuidanceSessionRule, Guid> ruleRepository,
        IRepository<AdvisingRequest, Guid> requestRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        GuidanceManager guidanceManager,
        IDistributedEventBus distributedEventBus,
        AlumniApplicationMappers alumniMappers)
    {
        _ruleRepository = ruleRepository;
        _requestRepository = requestRepository;
        _profileRepository = profileRepository;
        _guidanceManager = guidanceManager;
        _distributedEventBus = distributedEventBus;
        _alumniMappers = alumniMappers;
    }

    /* OBSOLETE: GuidanceSessionRule is now per-Branch, not per-Advisor.
    [Authorize(AlumniPermissions.Guidance.ManageAvailability)]
    public async Task<GuidanceSessionRuleDto> CreateRuleAsync(CreateUpdateGuidanceSessionRuleDto input)
    {
        var rule = new GuidanceSessionRule(
            GuidGenerator.Create(),
            CurrentUser.GetId(),
            input.DayOfWeek,
            input.StartTime,
            input.EndTime
        );

        await _ruleRepository.InsertAsync(rule);
        return _alumniMappers.MapToDto(rule);
    }

    public async Task<List<GuidanceSessionRuleDto>> GetRulesAsync(Guid advisorId)
    {
        var rules = await _ruleRepository.GetListAsync(x => x.AdvisorId == advisorId && x.IsActive);
        return _alumniMappers.MapToDtos(rules);
    }

    [Authorize(AlumniPermissions.Guidance.ManageAvailability)]
    public async Task DeleteRuleAsync(Guid id)
    {
        var rule = await _ruleRepository.GetAsync(id);
        if (rule.AdvisorId != CurrentUser.GetId())
        {
            throw new UserFriendlyException("You can only delete your own availability rules.");
        }
        await _ruleRepository.DeleteAsync(rule);
    }
    */

    [Authorize(AlumniPermissions.Guidance.BookSession)]
    public async Task<AdvisingRequestDto> BookSessionAsync(BookSessionDto input)
    {
        await _guidanceManager.ValidateSlotAsync(input.AdvisorId, input.StartTime, input.EndTime);

        var advisor = await _profileRepository.GetAsync(x => x.UserId == input.AdvisorId); // Assuming AdvisorId is UserId (Standard in this codebase) OR ProfileId. 
        // GuidanceSessionRule uses "AdvisorId". Usually in this system "AdvisorId" refers to userId unless specified. 
        // Checking GuidanceSessionRuleDto creation: "CurrentUser.GetId()". So AdvisorId is UserId.
        // But AlumniProfile has UserId... wait. AlumniProfile.Id is ProfileId.
        // If "input.AdvisorId" is UserId, we need to find Profile where UserId == AdvisorId.
        
        // Actually, let's assume input.AdvisorId is the ProfileId for safety or UserId? 
        // GuidanceSessionRule.AdvisorId = CurrentUser.GetId(); -> UserId.
        // So matches inputs.
        
        var request = new AdvisingRequest(
            GuidGenerator.Create(),
            CurrentUser.GetId(),
            advisor.BranchId,
            input.AdvisorId,
            input.StartTime,
            input.EndTime,
            input.Subject
        ) { Description = input.Description };

        await _requestRepository.InsertAsync(request);

        // Publish ETO for notification
        await _distributedEventBus.PublishAsync(new SessionRequestedEto
        {
            AdvisingRequestId = request.Id,
            AlumniId = request.AlumniId,
            AdvisorId = request.AdvisorId,
            RequestedTime = request.StartTime
        });

        return _alumniMappers.MapToDto(request);
    }

    public async Task<PagedResultDto<AdvisingRequestDto>> GetMyRequestsAsync(PagedAndSortedResultRequestDto input)
    {
        var currentUserId = CurrentUser.GetId();
        var query = await _requestRepository.GetQueryableAsync();
        
        query = query.Where(x => x.AlumniId == currentUserId || x.AdvisorId == currentUserId);
        
        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.PageBy(input.SkipCount, input.MaxResultCount));
        
        return new PagedResultDto<AdvisingRequestDto>(totalCount, _alumniMappers.MapToDtos(items));
    }

    [Authorize(AlumniPermissions.Guidance.ManageRequests)]
    public async Task<AdvisingRequestDto> UpdateRequestStatusAsync(Guid id, AdvisingRequestStatus status)
    {
        var request = await _requestRepository.GetAsync(id);
        
        if (request.AdvisorId != CurrentUser.GetId())
        {
            throw new UserFriendlyException("You are not the advisor for this request.");
        }

        request.SetStatus(status);
        await _requestRepository.UpdateAsync(request);
        
        return _alumniMappers.MapToDto(request);
    }
}
