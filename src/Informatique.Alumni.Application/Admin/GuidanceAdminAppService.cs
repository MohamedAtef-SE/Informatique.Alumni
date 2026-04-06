using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Emailing;
using Microsoft.AspNetCore.Mvc;
using Informatique.Alumni.Guidance;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.GuidanceManage)]
public class GuidanceAdminAppService : AlumniAppService, IGuidanceAdminAppService
{
    private readonly IRepository<AdvisingRequest, Guid> _requestRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IRepository<GuidanceSessionRule, Guid> _ruleRepository;

    private readonly Volo.Abp.EventBus.Local.ILocalEventBus _localEventBus;

    public GuidanceAdminAppService(
        IRepository<AdvisingRequest, Guid> requestRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IIdentityUserRepository userRepository,
        Volo.Abp.EventBus.Local.ILocalEventBus localEventBus,
        IRepository<GuidanceSessionRule, Guid> ruleRepository)
    {
        _requestRepository = requestRepository;
        _profileRepository = profileRepository;
        _userRepository = userRepository;
        _localEventBus = localEventBus;
        _ruleRepository = ruleRepository;
    }

    public async Task<PagedResultDto<GuidanceAdminDto>> GetListAsync(GuidanceAdminGetListInput input)
    {
        var queryable = await _requestRepository.GetQueryableAsync();

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => (int)x.Status == input.Status.Value);
        }

        if (input.MinDate.HasValue)
        {
            queryable = queryable.Where(x => x.StartTime >= input.MinDate.Value);
        }

        if (input.MaxDate.HasValue)
        {
            queryable = queryable.Where(x => x.StartTime <= input.MaxDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x => x.Subject.Contains(input.Filter));
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var requests = await AsyncExecuter.ToListAsync(queryable);

        // Batch resolve Profiles for both Alumni and Advisors
        var profileIds = requests.Select(x => x.AlumniId)
                                 .Union(requests.Select(x => x.AdvisorId))
                                 .Distinct()
                                 .ToList();
                                 
        var profiles = await _profileRepository.GetListAsync(x => profileIds.Contains(x.Id));
        
        // Resolve Identity User Ids to get Names
        var userIds = profiles.Select(p => p.UserId).Distinct().ToList();
        var users = await _userRepository.GetListByIdsAsync(userIds);

        var items = requests.Select(r =>
        {
            var alumniProfile = profiles.FirstOrDefault(p => p.Id == r.AlumniId);
            var alumniUser = alumniProfile != null ? users.FirstOrDefault(u => u.Id == alumniProfile.UserId) : null;
            
            var advisorProfile = profiles.FirstOrDefault(p => p.Id == r.AdvisorId);
            var advisorUser = advisorProfile != null ? users.FirstOrDefault(u => u.Id == advisorProfile.UserId) : null;

            var alumniName = "Unknown Alumni";
            if (alumniUser != null)
            {
                if (alumniUser.ExtraProperties.TryGetValue("Name", out var nObj) && nObj != null)
                    alumniName = nObj.ToString();
                else if (!string.IsNullOrWhiteSpace(alumniUser.Name))
                    alumniName = $"{alumniUser.Name} {alumniUser.Surname}".Trim();
                else
                    alumniName = alumniUser.UserName ?? "Unknown Alumni";
            }

            var advName = "Unknown Advisor";
            if (advisorUser != null)
            {
                if (advisorUser.ExtraProperties.TryGetValue("Name", out var nObj) && nObj != null)
                    advName = nObj.ToString();
                else if (!string.IsNullOrWhiteSpace(advisorUser.Name))
                    advName = $"{advisorUser.Name} {advisorUser.Surname}".Trim();
                else
                    advName = advisorUser.UserName ?? "Unknown Advisor";
            }

            return new GuidanceAdminDto
            {
                Id = r.Id,
                AlumniId = r.AlumniId,
                AlumniName = alumniName,
                AlumniEmail = alumniUser?.Email ?? string.Empty,
                AdvisorId = r.AdvisorId,
                AdvisorName = advName,
                AdvisorEmail = advisorUser?.Email ?? string.Empty,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Subject = r.Subject,
                Notes = r.AdminNotes,
                MeetingLink = r.MeetingLink,
                Status = (int)r.Status,
                CreationTime = r.CreationTime
            };
        }).ToList();

        return new PagedResultDto<GuidanceAdminDto>(totalCount, items);
    }

    public async Task ApproveRequestAsync(Guid id, ApproveGuidanceRequestDto input)
    {
        var request = await _requestRepository.GetAsync(id);
        var oldStatus = request.Status;
        
        request.Approve(input.MeetingLink);
        await _requestRepository.UpdateAsync(request);

        await _localEventBus.PublishAsync(new AdvisingRequestStatusChangedEto
        {
            RequestId = request.Id,
            AlumniId = request.AlumniId,
            OldStatus = oldStatus,
            NewStatus = request.Status
        });
    }

    public async Task RejectRequestAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        var oldStatus = request.Status;

        request.Reject("Rejected by admin");
        await _requestRepository.UpdateAsync(request);

        await _localEventBus.PublishAsync(new AdvisingRequestStatusChangedEto
        {
            RequestId = request.Id,
            AlumniId = request.AlumniId,
            OldStatus = oldStatus,
            NewStatus = request.Status
        });
    }

    [Authorize(AlumniPermissions.Guidance.ManageAvailability)]
    [HttpGet("/api/app/guidance-admin/rule")]
    public async Task<GuidanceSessionRuleDto> GetRuleAsync(Guid branchId)
    {
        var ruleQuery = await _ruleRepository.WithDetailsAsync(x => x.WeekDays);
        var rule = await AsyncExecuter.FirstOrDefaultAsync(ruleQuery.Where(x => x.BranchId == branchId));

        if (rule == null)
        {
            return new GuidanceSessionRuleDto { BranchId = branchId };
        }

        return new GuidanceSessionRuleDto
        {
            Id = rule.Id,
            BranchId = rule.BranchId,
            StartTime = rule.StartTime,
            EndTime = rule.EndTime,
            SessionDurationMinutes = rule.SessionDurationMinutes,
            WeekDays = rule.WeekDays.Select(w => w.Day).ToList()
        };
    }

    [Authorize(AlumniPermissions.Guidance.ManageAvailability)]
    [HttpPost("/api/app/guidance-admin/rule")]
    public async Task SaveRuleAsync(UpdateGuidanceSessionRuleDto input)
    {
        var ruleQuery = await _ruleRepository.WithDetailsAsync(x => x.WeekDays);
        var rule = await AsyncExecuter.FirstOrDefaultAsync(ruleQuery.Where(x => x.BranchId == input.BranchId));

        if (rule == null)
        {
            rule = new GuidanceSessionRule(
                GuidGenerator.Create(),
                input.BranchId,
                input.StartTime,
                input.EndTime,
                input.SessionDurationMinutes
            );

            foreach (var day in input.WeekDays)
            {
                rule.AddWeekDay(GuidGenerator.Create(), day);
            }

            await _ruleRepository.InsertAsync(rule);
        }
        else
        {
            rule.UpdateTimeWindow(input.StartTime, input.EndTime, input.SessionDurationMinutes);
            rule.ClearWeekDays();

            foreach (var day in input.WeekDays)
            {
                rule.AddWeekDay(GuidGenerator.Create(), day);
            }

            await _ruleRepository.UpdateAsync(rule);
        }
    }
}
