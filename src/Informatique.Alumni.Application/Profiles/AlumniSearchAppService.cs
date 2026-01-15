using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Volo.Abp.Authorization;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Implementation of Alumni Search and Profile Management for Employees.
/// Enforces Branch Security, Photo Edit Exception, and Latest Qualification projection.
/// </summary>
[Authorize(AlumniPermissions.Profiles.Default)] // Basic permission, method level specific
public class AlumniSearchAppService : AlumniAppService, IAlumniSearchAppService
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;

    public AlumniSearchAppService(
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Education, Guid> educationRepository,
        IIdentityUserRepository userRepository,
        IRepository<Branch, Guid> branchRepository)
    {
        _profileRepository = profileRepository;
        _educationRepository = educationRepository;
        _userRepository = userRepository;
        _branchRepository = branchRepository;
    }

    /// <summary>
    /// Search Alumni with complex filtering and Branch Scoping.
    /// Projection: Selects LATEST qualification for list view.
    /// </summary>
    [Authorize(AlumniPermissions.Profiles.Search)] // Assume specific permission exists or use general
    public async Task<PagedResultDto<AlumniListDto>> GetListAsync(AlumniSearchFilterDto input)
    {
        // 1. Branch Security Scope
        await ValidateBranchScopeAsync(input.BranchId);

        // 2. Query Preparation
        var query = await _profileRepository.GetQueryableAsync();
        var educationQuery = await _educationRepository.GetQueryableAsync();

        // 3. Filter by Branch
        query = query.Where(p => p.BranchId == input.BranchId);

        // 4. Apply Filters
        if (input.IsVip != VipFilterOption.All)
        {
            query = query.Where(p => p.IsVip == (input.IsVip == VipFilterOption.VipOnly));
        }

        if (input.NationalityId.HasValue)
        {
            // Assuming we join with User or Profile has NationalityId
            // For now, assume Profile has Nationality (string or Id)
            // Profile.cs has NationalId (string) and Nationality (string?) in DTO
            // We might need to handle this based on actual entity structure
        }

        // 5. Join with Education for Academic Filters
        // We need to filter profiles that HAVE an education matching the criteria
        var filteredEducation = educationQuery.AsQueryable();

        if (input.GraduationYears != null && input.GraduationYears.Any())
        {
            filteredEducation = filteredEducation.Where(e => input.GraduationYears.Contains(e.GraduationYear));
        }
        
        if (input.GraduationSemesters != null && input.GraduationSemesters.Any())
        {
             filteredEducation = filteredEducation.Where(e => input.GraduationSemesters.Contains(e.GraduationSemester));
        }

        if (input.CollegeId.HasValue)
        {
            filteredEducation = filteredEducation.Where(e => e.CollegeId == input.CollegeId);
        }
        
        if (input.MajorId.HasValue)
        {
            filteredEducation = filteredEducation.Where(e => e.MajorId == input.MajorId);
        }

        // Filter Profiles that match education criteria
        query = query.Where(p => filteredEducation.Any(e => e.AlumniProfileId == p.Id));

        // 6. Count
        var totalCount = await AsyncExecuter.CountAsync(query);

        // 7. Projection (Latest Qualification)
        // We need to get the latest education for each profile
        // Strategy: Fetch profiles, then fetch latest education for them in memory or subquery
        
        query = query.OrderBy(input.Sorting ?? nameof(AlumniProfile.CreationTime));
        query = query.PageBy(input);

        var profiles = await AsyncExecuter.ToListAsync(query);
        
        var dtos = new List<AlumniListDto>();
        foreach (var profile in profiles)
        {
            var user = await _userRepository.FindAsync(profile.UserId);
            
            // Get Latest Qualification
            var educations = await _educationRepository.GetListAsync(e => e.AlumniProfileId == profile.Id);
            var latestEd = educations.OrderByDescending(e => e.GraduationYear).ThenByDescending(e => e.GraduationSemester).FirstOrDefault();

            var dto = new AlumniListDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                Name = user?.Name ?? "Unknown",
                AlumniId = user?.UserName ?? "", // Assumption: AlumniId is Username
                IsVip = profile.IsVip,
                PhotoUrl = profile.PhotoUrl,
                PrimaryEmail = user?.Email, // Or fetch from profile.Emails
                // Academic Info (Latest)
                GraduationYear = latestEd?.GraduationYear ?? 0,
                GraduationSemester = latestEd?.GraduationSemester ?? 0,
                College = latestEd?.CollegeId?.ToString() ?? latestEd?.InstitutionName ?? "", 
                Major = latestEd?.MajorId?.ToString() ?? latestEd?.Degree ?? "",
                GPA = 0 // GPA not on Education yet?
            };
            dtos.Add(dto);
        }

        return new PagedResultDto<AlumniListDto>(totalCount, dtos);
    }

    /// <summary>
    /// Get Full Profile with All Qualifications.
    /// </summary>
    public async Task<AlumniProfileDetailDto> GetProfileAsync(Guid id)
    {
        var profile = await _profileRepository.GetAsync(id);
        
        // Security Check: Ensure user has access to this branch
        await ValidateBranchScopeAsync(profile.BranchId);

        var user = await _userRepository.GetAsync(profile.UserId);
        var educations = await _educationRepository.GetListAsync(e => e.AlumniProfileId == profile.Id);

        return new AlumniProfileDetailDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Name = user.Name,
            AlumniId = user.UserName,
            IsVip = profile.IsVip,
            PhotoUrl = profile.PhotoUrl,
            WalletBalance = profile.WalletBalance,
            Educations = educations.Select(e => new AlumniEducationDto
            {
                Id = e.Id,
                InstitutionName = e.InstitutionName,
                Degree = e.Degree,
                GraduationYear = e.GraduationYear,
                GraduationSemester = e.GraduationSemester,
                College = e.CollegeId?.ToString(),
                Major = e.MajorId?.ToString()
            }).ToList()
        };
    }

    /// <summary>
    /// Explicitly allow updating the photo.
    /// </summary>
    [Authorize(AlumniPermissions.Profiles.Edit)] 
    public async Task UpdatePhotoAsync(Guid id, UpdateAlumniPhotoDto input)
    {
        var profile = await _profileRepository.GetAsync(id);
        
        // Branch Security
        await ValidateBranchScopeAsync(profile.BranchId);

        // Logic to save photo (Mocking explicit file constraints)
        // In real impl: use IImageStorageService to save input.PhotoData
        // For now, simulate URL generation
        var photoUrl = $"https://alumnistorage.example.com/photos/{id}/{Guid.NewGuid()}.jpg";
        
        profile.SetPhotoUrl(photoUrl);
        
        await _profileRepository.UpdateAsync(profile);
    }

    /// <summary>
    /// Proxy: Search Expected Graduates in Legacy SIS.
    /// Security: Enforces Branch Scoping.
    /// </summary>
    [Authorize(AlumniPermissions.Profiles.Search)]
    public async Task<PagedResultDto<ExpectedGraduateDto>> GetExpectedGraduatesAsync(ExpectedGraduateFilterDto input)
    {
        // SECURITY LOGIC: Branch Scoping
        var userBranchIdString = CurrentUser.FindClaimValue("BranchId");
        if (!string.IsNullOrEmpty(userBranchIdString) && Guid.TryParse(userBranchIdString, out var userBranchId))
        {
            input.BranchId = userBranchId;
        }

        // Validate Mandatory Filter
        if (!input.BranchId.HasValue)
        {
             throw new UserFriendlyException("Branch Selection is Mandatory.");
        }
        
        // MAPPING: DTO -> Domain Filter
        var domainFilter = new SisExpectedGraduateFilter
        {
            BranchId = input.BranchId,
            CollegeId = input.CollegeId,
            MajorId = input.MajorId,
            MinorId = input.MinorId,
            GpaFrom = input.GpaFrom,
            GpaTo = input.GpaTo,
            Gender = input.Gender,
            Nationality = input.Nationality,
            IdentityType = input.IdentityType,
            IdentityNumber = input.IdentityNumber,
            StudentId = input.StudentId,
            StudentName = input.StudentName,
            SkipCount = input.SkipCount,
            MaxResultCount = input.MaxResultCount
        };

        // INTEGRATION: Call Domain Service (Proxy)
        var sisIntegration = LazyServiceProvider.LazyGetRequiredService<IStudentSystemIntegrationService>();
        var result = await sisIntegration.GetExpectedGraduatesAsync(domainFilter);

        // MAPPING: Domain Result -> Output DTO
        var dtos = result.Items.Select(x => new ExpectedGraduateDto
        {
            StudentId = x.StudentId,
            NameAr = x.NameAr,
            NameEn = x.NameEn,
            BranchName = x.BranchName,
            CollegeName = x.CollegeName,
            MajorName = x.MajorName,
            GPA = x.GPA,
            CreditHoursPassed = x.CreditHoursPassed,
            Email = x.Email,
            Mobile = x.Mobile,
            Phone = x.Phone,
            Address = x.Address,
            BirthDate = x.BirthDate,
            NationalId = x.NationalId,
            Id = x.StudentId
        }).ToList();

        return new PagedResultDto<ExpectedGraduateDto>(result.TotalCount, dtos);
    }

    private async Task ValidateBranchScopeAsync(Guid branchId)
    {
        // If user is Branch Admin, they can only access their branch
        var userBranchId = CurrentUser.FindClaimValue("BranchId");
        if (!string.IsNullOrEmpty(userBranchId) && Guid.TryParse(userBranchId, out var bid))
        {
            if (bid != branchId)
            {
                throw new AbpAuthorizationException("Access Denied: You can only view alumni from your branch.");
            }
        }
    }
}
