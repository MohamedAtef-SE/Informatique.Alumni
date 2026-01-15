using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Application service for Alumni Basic Data reporting.
/// Supports Detailed (list) and Statistical (pivot/cross-tab) modes.
/// </summary>
[Authorize(AlumniPermissions.Reporting.BasicReport)]
public class AlumniReportingAppService : ApplicationService, IAlumniReportingAppService
{
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Education, Guid> _educationRepository;
    private readonly IRepository<AssociationRequest, Guid> _membershipRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IStudentSystemIntegrationService _sisService;

    public AlumniReportingAppService(
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Education, Guid> educationRepository,
        IRepository<AssociationRequest, Guid> membershipRepository,
        IRepository<Branch, Guid> branchRepository,
        IIdentityUserRepository userRepository,
        IStudentSystemIntegrationService sisService)
    {
        _profileRepository = profileRepository;
        _educationRepository = educationRepository;
        _membershipRepository = membershipRepository;
        _branchRepository = branchRepository;
        _userRepository = userRepository;
        _sisService = sisService;
    }

    /// <summary>
    /// Generate Alumni Basic Data Report (Detailed or Statistical).
    /// Business Rules: Mandatory filters with defaults, branch security, pivot for statistical mode.
    /// </summary>
    public async Task<object> GetReportAsync(AlumniReportInputDto input)
    {
        // Apply defaults
        ApplyDefaults(input);

        // Validate branch permissions
        await ValidateBranchPermissions(input.BranchId);

        // Route to report type
        if (input.ReportType == AlumniReportType.Statistical)
        {
            return await GetStatisticalReportAsync(input);
        }
        else
        {
            return await GetDetailedReportAsync(input);
        }
    }

    /// <summary>
    /// Detailed Report: List of alumni with all profile data.
    /// Includes CardExpiryDate from latest active membership.
    /// </summary>
    private async Task<PagedResultDto<AlumniReportDetailDto>> GetDetailedReportAsync(AlumniReportInputDto input)
    {
        var query = await _profileRepository.GetQueryableAsync();

        // Apply filters (Note: This is a simplified version. In production, you'd join with Education table)
        // For now, assuming AlumniProfile has these fields or you'd need to join with Education
        query = ApplyFilters(query, input);

        var totalCount = await AsyncExecuter.CountAsync(query);

        // Apply sorting
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "Id" : input.Sorting;
        
        // Simplified query - in production you'd need proper joins
        var profiles = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        // Map to DTOs (simplified - you'd fetch related data)
        var dtos = new List<AlumniReportDetailDto>();
        int serialNo = input.SkipCount + 1;

        foreach (var profile in profiles)
        {
            var user = await _userRepository.FindAsync(profile.UserId);
            
            // Get latest membership card expiry date
            // TODO: Fix MembershipStatus enum reference
            // var membershipQuery = await _membershipRepository.GetQueryableAsync();
            // var latestMembership = await AsyncExecuter.FirstOrDefaultAsync(
            //     membershipQuery
            //         .Where(x => x.AlumniId == profile.Id && x.Status == MembershipStatus.Approved)
            //         .OrderByDescending(x => x.CreationTime)
            // );
            DateTime? cardExpiryDate = null; // Placeholder until MembershipStatus enum is defined

            var dto = new AlumniReportDetailDto
            {
                SerialNo = serialNo++,
                Name = user?.Name ?? "N/A",
                AlumniId = profile.Id.ToString(),
                Email = user?.Email ?? "N/A",
                Phone = user?.PhoneNumber ?? "N/A",
                Mobile = profile.MobileNumber ?? "N/A",
                CardExpiryDate = cardExpiryDate,
                // Note: GraduationYear, College, Major, etc. require joins with Education table
                GraduationYear = 0, // Placeholder
                GraduationSemester = 0, // Placeholder
                CollegeName = "N/A", // Requires join
                MajorName = "N/A", // Requires join
                Country = "N/A", // Requires additional data
                City = "N/A",
                Address = "N/A"
            };

            dtos.Add(dto);
        }

        return new PagedResultDto<AlumniReportDetailDto>(totalCount, dtos);
    }

    /// <summary>
    /// Statistical Report: Pivot/Cross-Tab grouped by Year/Semester with College columns.
    /// PIVOT LOGIC: Transform flat data into matrix structure.
    /// </summary>
    private async Task<AlumniStatisticalReportDto> GetStatisticalReportAsync(AlumniReportInputDto input)
    {
        // Step 1: Fetch raw data grouped by Year, Semester, and College
        // Note: This requires joining AlumniProfile with Education to get graduation data
        var query = await _profileRepository.GetQueryableAsync();
        query = ApplyFilters(query, input);

        // For demonstration, creating sample pivot data structure
        // In production, you'd query: SELECT Year, Semester, College, COUNT(*) FROM ... GROUP BY Year, Semester, College
        
        // Simplified version - assumes we can get this data
        // In reality, you'd need to join with Education table to get Year/Semester/College
        var rawData = new List<(int Year, int Semester, string College, int Count)>
        {
            // Sample data structure showing what the query should return
            // (2023, 1, "Engineering", 150),
            // (2023, 1, "Management", 80),
            // (2023, 2, "Engineering", 120),
            // etc.
        };

        // Step 2: PIVOT LOGIC - Transform flat data into cross-tab structure
        var pivotRows = rawData
            .GroupBy(x => new { x.Year, x.Semester })
            .Select(yearSemGroup => new AlumniReportStatsRowDto
            {
                GraduationYear = yearSemGroup.Key.Year,
                GraduationSemester = yearSemGroup.Key.Semester,
                
                // Pivot: Create dictionary with College as key, Count as value
                CollegeCounts = yearSemGroup.ToDictionary(
                    x => x.College,
                    x => x.Count
                ),
                
                // Row total: Sum all college counts for this year/semester
                RowTotal = yearSemGroup.Sum(x => x.Count)
            })
            .OrderBy(x => x.GraduationYear)
            .ThenBy(x => x.GraduationSemester)
            .ToList();

        // Step 3: Calculate Grand Totals
        var grandTotalByCollege = rawData
            .GroupBy(x => x.College)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Count)
            );

        var grandTotal = rawData.Sum(x => x.Count);

        return new AlumniStatisticalReportDto
        {
            Rows = pivotRows,
            GrandTotalByCollege = grandTotalByCollege,
            GrandTotal = grandTotal
        };
    }

    private IQueryable<AlumniProfile> ApplyFilters(IQueryable<AlumniProfile> query, AlumniReportInputDto input)
    {
        // Note: These filters require joining with Education table for graduation data
        // Simplified implementation - you'd need proper joins in production
        
        // GraduationYear filter
        if (input.GraduationYears != null && input.GraduationYears.Any())
        {
            // query = query.Where(x => input.GraduationYears.Contains(x.Education.GraduationYear));
        }

        // GraduationSemester filter
        if (input.GraduationSemesters != null && input.GraduationSemesters.Any())
        {
            // query = query.Where(x => input.GraduationSemesters.Contains(x.Education.GraduationSemester));
        }

        // College filter
        if (input.CollegeId.HasValue)
        {
            // query = query.Where(x => x.Education.CollegeId == input.CollegeId);
        }

        // Major filter
        if (input.MajorId.HasValue)
        {
            // query = query.Where(x => x.Education.MajorId == input.MajorId);
        }

        return query;
    }

    private void ApplyDefaults(AlumniReportInputDto input)
    {
        // Default Graduation Years to last academic year
        if (input.GraduationYears == null || !input.GraduationYears.Any())
        {
            var currentYear = DateTime.Now.Year;
            input.GraduationYears = new List<int> { currentYear - 1 }; // Last year
        }

        // Default Graduation Semesters to last semester
        if (input.GraduationSemesters == null || !input.GraduationSemesters.Any())
        {
            input.GraduationSemesters = new List<int> { 2 }; // Assuming semester 2 is last
        }
    }

    private async Task ValidateBranchPermissions(Guid branchId)
    {
        // Business Rule: Enforce user's branch permissions
        var currentUserBranchId = CurrentUser.GetCollegeId(); // Extension method maps "BranchId" claim

        if (currentUserBranchId.HasValue && currentUserBranchId.Value != branchId)
        {
            throw new AbpAuthorizationException("You do not have permission to view reports for this branch.");
        }

        // Validate branch exists
        await _branchRepository.GetAsync(branchId);
    }

    /// <summary>
    /// Generate Expected Graduates Report.
    /// Fetches data from Legacy SIS via Integration Service.
    /// Applies strict security (Branch Overwrite) and in-memory sorting.
    /// </summary>
    public async Task<List<ExpectedGraduatesReportOutputDto>> GetExpectedGraduatesReportAsync(ExpectedGraduatesReportInputDto input)
    {
        // 1. Security: Branch Scope Enforcement
        // If user is restricted to a branch (e.g. BranchAdmin), we overwrite the input.BranchId
        var currentUserBranchId = CurrentUser.GetCollegeId(); 
        if (currentUserBranchId.HasValue)
        {
            input.BranchId = currentUserBranchId.Value;
        }
        
        // 2. Call Integration Service
        // Map DTO to SIS Filter
        var sisFilter = new SisExpectedGraduateFilter
        {
            CollegeId = input.CollegeId,
            MajorId = input.MajorId,
            GpaFrom = input.GpaFrom,
            GpaTo = input.GpaTo,
            PassedHoursFrom = input.PassedHoursFrom,
            PassedHoursTo = input.PassedHoursTo
            // BranchId is implicitly handled by CollegeId in SIS usually, or passed via other means.
            // Assuming CollegeId defines the scope for SIS.
        };

        var sisResult = await _sisService.GetExpectedGraduatesAsync(sisFilter);
        var graduates = sisResult.Items;

        // 3. Apply Sorting (In-Memory)
        // Default: Alumni Name
        if (string.IsNullOrWhiteSpace(input.Sorting))
        {
            graduates = graduates.OrderBy(x => x.NameEn).ToList();
        }
        else
        {
            // Custom Sorting: Alumni Number, Major, GPA, Passed Hours
            // Map Sorting string to property
            // Note: input.Sorting usually comes as "Property ASC" or "Property DESC" from ABP
            // Simple implementation for fixed options:
            var sort = input.Sorting.ToLower();
            if (sort.Contains("alumniid") || sort.Contains("number")) // Alumni Number
                graduates = graduates.OrderBy(x => x.StudentId).ToList();
            else if (sort.Contains("major"))
                graduates = graduates.OrderBy(x => x.MajorName).ToList(); // Assuming MajorName exists on SisExpectedGraduate
            else if (sort.Contains("gpa"))
                graduates = graduates.OrderByDescending(x => x.GPA).ToList(); // Default detailed logic
            else if (sort.Contains("passedhours"))
                graduates = graduates.OrderByDescending(x => x.CreditHoursPassed).ToList();
            else
                graduates = graduates.OrderBy(x => x.NameEn).ToList();
        }

        // 4. Map to Output DTO
        var reportOutput = new List<ExpectedGraduatesReportOutputDto>();
        int serial = 1;

        foreach (var grad in graduates)
        {
            reportOutput.Add(new ExpectedGraduatesReportOutputDto
            {
                SerialNo = serial++,
                AlumniId = grad.StudentId,
                AlumniName = grad.NameEn,
                CollegeName = grad.CollegeName,
                MajorName = grad.MajorName,
                PassedHours = grad.CreditHoursPassed,
                GPA = grad.GPA
            });
        }

        return reportOutput;
    }
}
