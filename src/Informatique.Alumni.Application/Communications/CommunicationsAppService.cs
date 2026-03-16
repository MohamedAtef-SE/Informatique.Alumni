using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Communications;

[Authorize(AlumniPermissions.Communication.SendMassMessage)]
public class CommunicationsAppService : AlumniAppService, ICommunicationsAppService
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IRepository<Informatique.Alumni.Membership.AssociationRequest, Guid> _membershipRepository;
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly IRepository<CommunicationLog, Guid> _logRepository;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;

    public CommunicationsAppService(
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Education, Guid> educationRepository,
        IRepository<Informatique.Alumni.Membership.AssociationRequest, Guid> membershipRepository,
        IBackgroundJobManager backgroundJobManager,
        IRepository<CommunicationLog, Guid> logRepository,
        IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository)
    {
        _profileRepository = profileRepository;
        _educationRepository = educationRepository;
        _membershipRepository = membershipRepository;
        _backgroundJobManager = backgroundJobManager;
        _logRepository = logRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Preview count of recipients based on filter.
    /// </summary>
    [HttpPost]
    public async Task<int> GetRecipientsCountAsync(AlumniCommunicationFilterDto filter)
    {
        await ValidateBranchScopeAsync(filter.BranchId);
        
        var query = await BuildFilteredQueryAsync(filter);
        return await AsyncExecuter.CountAsync(query);
    }

    /// <summary>
    /// Send Message (Enqueue Background Job).
    /// </summary>
    [HttpPost]
    public async Task SendMessageAsync(SendGeneralMessageInputDto input)
    {
        await ValidateBranchScopeAsync(input.Filter.BranchId);

        if (input.Channel == CommunicationChannel.Email && string.IsNullOrWhiteSpace(input.Subject))
        {
            throw new UserFriendlyException("Subject is required for Email.");
        }
        if (string.IsNullOrWhiteSpace(input.Body))
        {
            throw new UserFriendlyException("Message Body is required.");
        }

        await _backgroundJobManager.EnqueueAsync(
            new GeneralMessageSenderJobArgs
            {
                Filter = input.Filter,
                Channel = input.Channel,
                Subject = input.Subject,
                Body = input.Body,
                AttachmentUrls = input.AttachmentUrls,
                SenderId = CurrentUser.GetId(),
                TenantId = CurrentTenant.Id
            }
        );
    }


    /// <summary>
    /// Get distinct graduation years from the Education table (for filter dropdowns).
    /// </summary>
    [HttpGet]
    public async Task<List<int>> GetDistinctGraduationYearsAsync()
    {
        var query = await _educationRepository.GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(
            query
                .Where(e => e.GraduationYear > 0)
                .Select(e => e.GraduationYear)
                .Distinct()
                .OrderByDescending(y => y)
        );
    }

    /// <summary>
    /// Get paginated communication delivery logs.
    /// </summary>
    [HttpPost]
    public async Task<Volo.Abp.Application.Dtos.PagedResultDto<CommunicationLogDto>> SearchLogsAsync(GetCommunicationLogInputDto input)
    {
        var logQuery = await _logRepository.GetQueryableAsync();
        var profileQuery = await _profileRepository.WithDetailsAsync(x => x.Emails, x => x.Mobiles);
        var userQuery = await _userRepository.GetQueryableAsync();

        if (input.RecipientId.HasValue)
            logQuery = logQuery.Where(x => x.RecipientId == input.RecipientId.Value);
        if (!string.IsNullOrWhiteSpace(input.Channel))
            logQuery = logQuery.Where(x => x.Channel == input.Channel);
        if (!string.IsNullOrWhiteSpace(input.Status))
            logQuery = logQuery.Where(x => x.Status == input.Status);

        var query = from log in logQuery
                    join profile in profileQuery on log.RecipientId equals profile.Id into profileGroup
                    from p in profileGroup.DefaultIfEmpty()
                    join user in userQuery on p.UserId equals user.Id into userGroup
                    from u in userGroup.DefaultIfEmpty()
                    select new { Log = log, Profile = p, User = u };

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            query = query.Where(x =>
                x.Log.Subject.Contains(input.FilterText) ||
                x.Log.Content.Contains(input.FilterText) ||
                (x.User != null && (x.User.Name.Contains(input.FilterText) || x.User.Surname.Contains(input.FilterText)))
            );
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        if (string.IsNullOrWhiteSpace(input.Sorting))
        {
            query = query.OrderByDescending(x => x.Log.CreationTime);
        }
        else
        {
            if (input.Sorting.Contains("Status", StringComparison.OrdinalIgnoreCase))
                query = query.OrderByDescending(x => x.Log.Status);
            else if (input.Sorting.Contains("Channel", StringComparison.OrdinalIgnoreCase))
                query = query.OrderByDescending(x => x.Log.Channel);
            else
                query = query.OrderByDescending(x => x.Log.CreationTime);
        }

        var pagedResults = await AsyncExecuter.ToListAsync(query.PageBy(input.SkipCount, input.MaxResultCount));

        var dtos = pagedResults.Select(x =>
        {
            string? targetAddress = null;
            if (x.Log.Channel == CommunicationChannel.Email.ToString() && x.Profile != null)
                targetAddress = x.Profile.Emails?.FirstOrDefault(e => e.IsPrimary)?.Email;
            else if (x.Profile != null)
                targetAddress = x.Profile.Mobiles?.FirstOrDefault(m => m.IsPrimary)?.MobileNumber;

            return new CommunicationLogDto
            {
                Id = x.Log.Id,
                SenderId = x.Log.SenderId,
                RecipientId = x.Log.RecipientId,
                Channel = x.Log.Channel,
                Subject = x.Log.Subject,
                Content = x.Log.Content,
                Status = x.Log.Status,
                ErrorMessage = x.Log.ErrorMessage,
                CreationTime = x.Log.CreationTime,
                AlumniName = x.User != null ? $"{x.User.Name} {x.User.Surname}".Trim() : "Unknown",
                TargetAddress = targetAddress
            };
        }).ToList();

        return new Volo.Abp.Application.Dtos.PagedResultDto<CommunicationLogDto>(totalCount, dtos);
    }

    private async Task ValidateBranchScopeAsync(Guid? branchId)
    {
        var userBranchIdString = CurrentUser.FindClaimValue("BranchId");
        if (!string.IsNullOrEmpty(userBranchIdString) && Guid.TryParse(userBranchIdString, out var userBranchId))
        {
            if (!branchId.HasValue || userBranchId != branchId.Value)
            {
                throw new AbpAuthorizationException("Access Denied: You can only communicate with alumni from your branch.");
            }
        }
    }

    private async Task<IQueryable<AlumniProfile>> BuildFilteredQueryAsync(AlumniCommunicationFilterDto filter)
    {
        var query = await _profileRepository.WithDetailsAsync(x => x.Educations);

        if (filter.BranchId.HasValue && filter.BranchId.Value != Guid.Empty)
        {
            query = query.Where(p => p.BranchId == filter.BranchId.Value);
        }

        if (filter.GraduationYear.HasValue)
        {
            query = query.Where(p => p.Educations.Any(e => e.GraduationYear == filter.GraduationYear));
        }

        if (filter.GraduationSemester.HasValue)
        {
            query = query.Where(p => p.Educations.Any(e => e.GraduationSemester == filter.GraduationSemester));
        }

        if (filter.CollegeId.HasValue)
        {
            query = query.Where(p => p.Educations.Any(e => e.CollegeId == filter.CollegeId));
        }

        if (filter.MajorId.HasValue)
        {
            query = query.Where(p => p.Educations.Any(e => e.MajorId == filter.MajorId));
        }

        if (filter.MinorId.HasValue)
        {
            query = query.Where(p => p.Educations.Any(e => e.MinorId == filter.MinorId));
        }

        if (filter.MembershipStatus.HasValue && filter.MembershipStatus != CommunicationMembershipStatus.All)
        {
            var activeAlumniIds = (await _membershipRepository.GetQueryableAsync())
                .Where(r => r.Status == Informatique.Alumni.Membership.MembershipRequestStatus.Approved)
                .Select(r => r.AlumniId);

            if (filter.MembershipStatus == CommunicationMembershipStatus.Active)
            {
                query = query.Where(p => activeAlumniIds.Contains(p.Id));
            }
            else
            {
                query = query.Where(p => !activeAlumniIds.Contains(p.Id));
            }
        }

        return query;
    }
}
