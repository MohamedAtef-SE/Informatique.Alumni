using System;
using System.Linq;
using System.Threading.Tasks;
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

    public GuidanceAdminAppService(
        IRepository<AdvisingRequest, Guid> requestRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IIdentityUserRepository userRepository)
    {
        _requestRepository = requestRepository;
        _profileRepository = profileRepository;
        _userRepository = userRepository;
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

        // Batch resolve Alumni Profiles
        var alumniIds = requests.Select(x => x.AlumniId).Distinct().ToList();
        var alumniProfiles = await _profileRepository.GetListAsync(x => alumniIds.Contains(x.Id));
        
        // Resolve Alumni User Ids to get Names
        var alumniUserIds = alumniProfiles.Select(p => p.UserId).Distinct().ToList();
        
        // Resolve Advisor IDs (assuming AdvisorId is User ID)
        var advisorIds = requests.Select(x => x.AdvisorId).Distinct().ToList();

        var allUserIds = alumniUserIds.Union(advisorIds).Distinct().ToList();
        var users = await _userRepository.GetListByIdsAsync(allUserIds);

        var items = requests.Select(r =>
        {
            var profile = alumniProfiles.FirstOrDefault(p => p.Id == r.AlumniId);
            var alumniUser = profile != null ? users.FirstOrDefault(u => u.Id == profile.UserId) : null;
            var advisorUser = users.FirstOrDefault(u => u.Id == r.AdvisorId);

            return new GuidanceAdminDto
            {
                Id = r.Id,
                AlumniId = r.AlumniId,
                AlumniName = alumniUser != null ? $"{alumniUser.Name} {alumniUser.Surname}" : "Unknown Alumni",
                AdvisorId = r.AdvisorId,
                AdvisorName = advisorUser != null ? $"{advisorUser.Name} {advisorUser.Surname}" : "Unknown Advisor",
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Subject = r.Subject,
                Notes = r.AdminNotes,
                Status = (int)r.Status,
                CreationTime = r.CreationTime
            };
        }).ToList();

        return new PagedResultDto<GuidanceAdminDto>(totalCount, items);
    }

    public async Task ApproveRequestAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        request.Approve();
        await _requestRepository.UpdateAsync(request);
    }

    public async Task RejectRequestAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        request.Reject("Rejected by admin");
        await _requestRepository.UpdateAsync(request);
    }
}
