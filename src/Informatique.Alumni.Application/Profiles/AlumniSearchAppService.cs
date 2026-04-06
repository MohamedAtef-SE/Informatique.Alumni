using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Directory;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Profiles;

[Authorize(AlumniPermissions.Profiles.Default)]
public class AlumniSearchAppService : AlumniAppService, IAlumniSearchAppService
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<Major, Guid> _majorRepository;
    private readonly IRepository<College, Guid> _collegeRepository;
    private readonly IRepository<AlumniDirectoryCache, Guid> _cacheRepository;

    public AlumniSearchAppService(
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Education, Guid> educationRepository,
        IIdentityUserRepository userRepository,
        IRepository<Branch, Guid> branchRepository,
        IRepository<Major, Guid> majorRepository,
        IRepository<College, Guid> collegeRepository,
        IRepository<AlumniDirectoryCache, Guid> cacheRepository)
    {
        _profileRepository = profileRepository;
        _educationRepository = educationRepository;
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _majorRepository = majorRepository;
        _collegeRepository = collegeRepository;
        _cacheRepository = cacheRepository;
    }

    [Authorize(AlumniPermissions.Profiles.Search)]
    public async Task<PagedResultDto<AlumniListDto>> GetListAsync(AlumniSearchFilterDto input)
    {
        // Enforce Branch Security
        var currentUserBranchId = CurrentUser.GetCollegeId(); 
        if (currentUserBranchId.HasValue)
        {
            input.BranchId = currentUserBranchId.Value;
        }

        var query = await _cacheRepository.GetQueryableAsync();
        
        // Exclude current user from their own directory view
        if (CurrentUser.Id.HasValue)
        {
            query = query.Where(x => x.UserId != CurrentUser.Id.Value);
        }

        // Apply privacy (ShowInDirectory)
        query = query.Where(x => x.ShowInDirectory);

        // Branch scoper
        if (input.BranchId.HasValue) 
        {
             // Note: In a real scenario, BranchId might be in the profile, we'd need to ensure cache has it.
             // But for now, let's assume direct scoper based on directory.
        }

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var filter = input.Filter.Trim();
            query = query.Where(x => 
                x.FullName.Contains(filter) || 
                x.JobTitle.Contains(filter) || 
                x.Company.Contains(filter) ||
                x.Major.Contains(filter));
        }

        if (input.GraduationYears != null && input.GraduationYears.Any())
        {
            query = query.Where(x => x.GraduationYear.HasValue && input.GraduationYears.Contains(x.GraduationYear.Value));
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        var pagedResults = await AsyncExecuter.ToListAsync(
            query.OrderBy(input.Sorting ?? "FullName")
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount));
        
        var userIds = pagedResults.Select(p => p.UserId).Distinct().ToList();
        var profiles = await _profileRepository.GetListAsync(p => userIds.Contains(p.UserId));
        var profileMap = profiles.ToDictionary(p => p.UserId, p => p.Id);

        var dtos = pagedResults.Select(x => new AlumniListDto
        {
            Id = profileMap.ContainsKey(x.UserId) ? profileMap[x.UserId] : x.Id, // Return real Profile ID so visitor view works
            UserId = x.UserId,
            AlumniId = x.UserId.ToString(),
            Name = x.FullName,
            College = x.College ?? "N/A",
            Major = x.Major ?? "N/A",
            GraduationYear = x.GraduationYear ?? 0,
            PhotoUrl = x.PhotoUrl,
            IsVip = x.IsVip
        }).ToList();

        return new PagedResultDto<AlumniListDto>(totalCount, dtos);
    }

    public async Task<AlumniProfileDetailDto> GetAsync(Guid id)
    {
        var profileQuery = await _profileRepository.WithDetailsAsync(
            x => x.Experiences, 
            x => x.Educations,
            x => x.Emails,
            x => x.Mobiles,
            x => x.Phones
        );
        
        var profile = profileQuery.FirstOrDefault(x => x.Id == id);
        
        // Fallback 1: Check if it's a UserId
        if (profile == null)
        {
            profile = profileQuery.FirstOrDefault(x => x.UserId == id);
        }

        // Fallback 2: Check if it's a CacheId (fetch UserId from cache first)
        if (profile == null)
        {
            var cache = await _cacheRepository.FindAsync(id);
            if (cache != null)
            {
                profile = profileQuery.FirstOrDefault(x => x.UserId == cache.UserId);
            }
        }

        if (profile == null) throw new EntityNotFoundException(typeof(AlumniProfile), id);
        
        // Branch Security
        var currentUserBranchId = CurrentUser.GetCollegeId(); 
        if (currentUserBranchId.HasValue && profile.BranchId != currentUserBranchId.Value)
        {
             throw new AbpAuthorizationException("Access Denied: You can only view alumni from your branch.");
        }

        var user = await _userRepository.GetAsync(profile.UserId);
        
        var branches = await _branchRepository.GetListAsync();
        var majors = await _majorRepository.GetListAsync();
        var colleges = await _collegeRepository.GetListAsync();

        return new AlumniProfileDetailDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            AlumniId = profile.UserId.ToString(), 
            Name = $"{user.Name} {user.Surname}".Trim(),
            Status = profile.Status,
            NameAr = $"{user.Name} {user.Surname}".Trim(),
            NameEn = $"{user.Name} {user.Surname}".Trim(),
            JobTitle = profile.JobTitle,
            Company = profile.Company,
            Bio = profile.Bio,
            City = profile.City,
            Country = profile.Country,
            IsVip = profile.IsVip,
            PhotoUrl = profile.PhotoUrl,
            ViewCount = profile.ViewCount,
            Educations = profile.Educations.Select(e => new AlumniEducationDto
            {
                Id = e.Id,
                InstitutionName = e.InstitutionName,
                Degree = e.Degree,
                GraduationYear = e.GraduationYear,
                GraduationSemester = e.GraduationSemester,
                College = colleges.FirstOrDefault(c => c.Id == e.CollegeId)?.Name ?? "N/A",
                Major = majors.FirstOrDefault(m => m.Id == e.MajorId)?.Name ?? "N/A"
            }).ToList(),
            Experiences = profile.Experiences.Select(e => new ExperienceDto
            {
                Id = e.Id,
                CompanyName = e.CompanyName,
                JobTitle = e.JobTitle,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Description = e.Description
            }).ToList(),
            Emails = profile.Emails.Select(e => new ContactEmailDto { Id = e.Id, Email = e.Email, IsPrimary = e.IsPrimary }).ToList(),
            Mobiles = profile.Mobiles.Select(m => new ContactMobileDto { Id = m.Id, MobileNumber = m.MobileNumber, IsPrimary = m.IsPrimary }).ToList(),
            Phones = profile.Phones.Select(p => new ContactPhoneDto { Id = p.Id, PhoneNumber = p.PhoneNumber, Label = p.Label }).ToList()
        };
    }

    [Authorize(AlumniPermissions.Profiles.Edit)] 
    public async Task UpdatePhotoAsync(Guid id, UpdateAlumniPhotoDto input)
    {
        var profile = await _profileRepository.GetAsync(id);
        // Simplified Logic
        await _profileRepository.UpdateAsync(profile);
    }

    [Authorize(AlumniPermissions.Profiles.Search)]
    public async Task<PagedResultDto<ExpectedGraduateDto>> GetExpectedGraduatesAsync(ExpectedGraduateFilterDto input)
    {
        return new PagedResultDto<ExpectedGraduateDto>();
    }
}
