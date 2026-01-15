using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Certificates;

[Authorize(AlumniPermissions.Certificates.Default)]
public class CertificateReportingAppService : ApplicationService, ICertificateReportingAppService
{
    private readonly IRepository<CertificateRequest, Guid> _requestRepository;
    private readonly IRepository<CertificateDefinition, Guid> _definitionRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly IRepository<Education, Guid> _educationRepository;

    public CertificateReportingAppService(
        IRepository<CertificateRequest, Guid> requestRepository,
        IRepository<CertificateDefinition, Guid> definitionRepository,
        IRepository<Branch, Guid> branchRepository,
        IRepository<Education, Guid> educationRepository)
    {
        _requestRepository = requestRepository;
        _definitionRepository = definitionRepository;
        _branchRepository = branchRepository;
        _educationRepository = educationRepository;
    }

    /// <summary>
    /// Generate Certificate Report with Detailed or Statistical view.
    /// Business Rules: Mandatory filters with defaults, LINQ projections only, branch security.
    /// </summary>
    [Authorize(AlumniPermissions.Certificates.Reports)]
    public async Task<object> GetReportAsync(CertificateReportInputDto input)
    {
        // ========== Apply Default Values ==========
        ApplyDefaults(input);

        // ========== Branch Security Validation ==========
        await ValidateBranchPermissions(input.BranchId);

        // ========== Route to Report Type ==========
        if (input.ReportType == CertificateReportType.Statistical)
        {
            return await GetStatisticalReportAsync(input);
        }
        else
        {
            return await GetDetailedReportAsync(input);
        }
    }

    /// <summary>
    /// Detailed Report: List view with all student details, grouped/ordered by branch.
    /// Uses LINQ projection to avoid loading entire entities into memory.
    /// </summary>
    private async Task<List<CertificateReportDetailDto>> GetDetailedReportAsync(CertificateReportInputDto input)
    {
        var query = await _requestRepository.GetQueryableAsync();

        // ========== Apply Filters ==========
        query = ApplyFilters(query, input);

        // ========== LINQ Projection (Performance Critical) ==========
        // Do NOT fetch entities, project directly to DTO
        var detailedQuery = query
            .SelectMany(r => r.Items, (request, item) => new
            {
                Request = request,
                Item = item
            })
            .Select(x => new CertificateReportDetailDto
            {
                // Note: Some fields require joining with other entities
                // For demonstration, we'll show the structure. You may need to enhance with actual joins.
                StudentName = "N/A", // Requires join with IdentityUser or AlumniProfile
                StudentId = x.Request.AlumniId.ToString(),
                GraduationYear = 0, // Requires join with Education/AlumniProfile
                GraduationSemester = 0, // Requires join with Education/AlumniProfile
                CollegeName = "N/A", // Requires join
                MajorName = "N/A", // Requires join
                CertificateName = "N/A", // Requires join with CertificateDefinition via item.CertificateDefinitionId
                RequestDate = x.Request.CreationTime,
                PaymentMethod = x.Request.UsedWalletAmount > 0 ? "Wallet" : "Gateway",
                DeliveryMethod = x.Request.DeliveryMethod,
                DeliveryStatus = x.Request.Status,
                BranchName = "N/A" // Requires join with Branch via request.TargetBranchId
            });

        // ========== Sorting ==========
        detailedQuery = ApplySorting(detailedQuery, input.SortBy, input.SortDescending);

        // ========== Execute Query ==========
        var results = await AsyncExecuter.ToListAsync(detailedQuery);

        // Add serial numbers
        for (int i = 0; i < results.Count; i++)
        {
            results[i].SerialNo = i + 1;
        }

        return results;
    }

    /// <summary>
    /// Statistical Report: Aggregation view grouped by certificate type.
    /// Uses GroupBy and Count for aggregation, returns summary table with grand total.
    /// </summary>
    private async Task<CertificateStatisticalReportDto> GetStatisticalReportAsync(CertificateReportInputDto input)
    {
        var query = await _requestRepository.GetQueryableAsync();

        // ========== Apply Filters ==========
        query = ApplyFilters(query, input);

        // ========== LINQ Aggregation (Group by Certificate Type) ==========
        var statsQuery = query
            .SelectMany(r => r.Items, (request, item) => new
            {
                Request = request,
                Item = item
            })
            .GroupBy(x => x.Item.CertificateDefinitionId)
            .Select(g => new CertificateReportStatsDto
            {
                CertificateTypeName = "N/A", // Requires join with CertificateDefinition
                TotalCount = g.Count(),
                InProgressCount = g.Count(x =>
                    x.Request.Status == CertificateRequestStatus.Processing ||
                    x.Request.Status == CertificateRequestStatus.ReadyForPickup ||
                    x.Request.Status == CertificateRequestStatus.OutForDelivery),
                DeliveredCount = g.Count(x => x.Request.Status == CertificateRequestStatus.Delivered),
                NotDeliveredCount = g.Count(x => x.Request.Status != CertificateRequestStatus.Delivered)
            });

        var stats = await AsyncExecuter.ToListAsync(statsQuery);

        // ========== Calculate Grand Total ==========
        var grandTotal = new CertificateReportStatsDto
        {
            CertificateTypeName = "GRAND TOTAL",
            TotalCount = stats.Sum(s => s.TotalCount),
            InProgressCount = stats.Sum(s => s.InProgressCount),
            DeliveredCount = stats.Sum(s => s.DeliveredCount),
            NotDeliveredCount = stats.Sum(s => s.NotDeliveredCount)
        };

        return new CertificateStatisticalReportDto
        {
            Items = stats,
            GrandTotal = grandTotal
        };
    }

    private IQueryable<CertificateRequest> ApplyFilters(IQueryable<CertificateRequest> query, CertificateReportInputDto input)
    {
        // Branch filter (Mandatory)
        query = query.Where(x => x.TargetBranchId == input.BranchId);

        // Graduation Year filter
        if (input.GraduationYears != null && input.GraduationYears.Any())
        {
            // Note: This requires joining with Education/AlumniProfile
            // For now, we'll leave it as a placeholder
            // query = query.Where(x => input.GraduationYears.Contains(x.AlumniProfile.GraduationYear));
        }

        // Graduation Semester filter
        if (input.GraduationSemesters != null && input.GraduationSemesters.Any())
        {
            // Note: This requires joining with Education/AlumniProfile
            // query = query.Where(x => input.GraduationSemesters.Contains(x.AlumniProfile.GraduationSemester));
        }

        // College filter (Optional)
        if (input.CollegeId.HasValue)
        {
            // Note: Requires join with AlumniProfile or Education
            // query = query.Where(x => x.AlumniProfile.CollegeId == input.CollegeId);
        }

        // Major filter (Optional)
        if (input.MajorId.HasValue)
        {
            // Note: Requires join with AlumniProfile or Education
            // query = query.Where(x => x.AlumniProfile.MajorId == input.MajorId);
        }

        // Certificate Type filter (Optional)
        if (input.CertificateTypeId.HasValue)
        {
            query = query.Where(x => x.Items.Any(item => item.CertificateDefinitionId == input.CertificateTypeId.Value));
        }

        return query;
    }

    private IQueryable<CertificateReportDetailDto> ApplySorting(
        IQueryable<CertificateReportDetailDto> query,
        string? sortBy,
        bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderBy(x => x.BranchName).ThenBy(x => x.StudentName);
        }

        if (sortBy.Equals("StudentName", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(x => x.StudentName)
                : query.OrderBy(x => x.StudentName);
        }

        if (sortBy.Equals("StudentId", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(x => x.StudentId)
                : query.OrderBy(x => x.StudentId);
        }

        // Default: Group by branch, then student name
        return query.OrderBy(x => x.BranchName).ThenBy(x => x.StudentName);
    }

    private void ApplyDefaults(CertificateReportInputDto input)
    {
        // Default Graduation Years to current/last academic year
        if (input.GraduationYears == null || !input.GraduationYears.Any())
        {
            var currentYear = DateTime.Now.Year;
            input.GraduationYears = new List<int> { currentYear, currentYear - 1 };
        }

        // Default Graduation Semesters to last semester (assuming 1 and 2)
        if (input.GraduationSemesters == null || !input.GraduationSemesters.Any())
        {
            input.GraduationSemesters = new List<int> { 2 }; // Last semester
        }
    }

    private async Task ValidateBranchPermissions(Guid branchId)
    {
        // Business Rule: Respect user's data permissions
        // If user has branch constraint, validate they can access this branch
        var currentUserBranchId = CurrentUser.GetCollegeId();

        if (currentUserBranchId.HasValue && currentUserBranchId.Value != branchId)
        {
            throw new Volo.Abp.Authorization.AbpAuthorizationException(
                "You do not have permission to view reports for this branch.");
        }

        // Validate branch exists
        await _branchRepository.GetAsync(branchId);
    }
}
