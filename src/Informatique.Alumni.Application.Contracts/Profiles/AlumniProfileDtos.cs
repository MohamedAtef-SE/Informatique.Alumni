using System;
using System.Collections.Generic;
using Informatique.Alumni.Guidance;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Profiles;

#region Enums

public enum AlumniReportType
{
    Detailed = 1,
    Statistical = 2
}

public enum VipFilterOption
{
    All = 0,
    VipOnly = 1,
    NonVipOnly = 2
}

#endregion

#region Core Profile DTOs

public class ContactEmailDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class ContactMobileDto
{
    public Guid Id { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class ContactPhoneDto
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Label { get; set; }
}

public class ExperienceDto : EntityDto<Guid>
{
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

public class EducationDto : EntityDto<Guid>
{
    public string InstitutionName { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
}

public class AlumniEducationDto : EntityDto<Guid>
{
    public string InstitutionName { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public int GraduationSemester { get; set; }
    public string? College { get; set; }
    public string? Major { get; set; }
}

public class EmployeeUpdateAlumniProfileDto
{
    public string? Bio { get; set; }
    public string? JobTitle { get; set; }
    public string? Address { get; set; }
    public byte[]? ProfilePhoto { get; set; }
    public List<ContactEmailDto> Emails { get; set; } = new();
    public List<ContactMobileDto> Mobiles { get; set; } = new();
    public List<ContactPhoneDto> Phones { get; set; } = new();
    public bool ShowInDirectory { get; set; }
}

public class ImportAlumniFromSisDto
{
    public string StudentId { get; set; } = string.Empty;
    public string OfficialEmail { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
}

#endregion

#region Academic History DTOs

public class AcademicHistoryDto
{
    public List<QualificationHistoryDto> Qualifications { get; set; } = new();
}

public class QualificationHistoryDto
{
    public string QualificationId { get; set; } = string.Empty;
    public string DegreeName { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public int GraduationYear { get; set; }
    public decimal CumulativeGPA { get; set; }
    public List<SemesterRecordDto> Semesters { get; set; } = new();
}

public class SemesterRecordDto
{
    public string SemesterCode { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int SemesterNumber { get; set; }
    public decimal SemesterGPA { get; set; }
    public int TotalCredits { get; set; }
    public List<CourseGradeDto> Courses { get; set; } = new();
}

public class CourseGradeDto
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string Grade { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public string InstructorName { get; set; } = string.Empty;
}

#endregion

#region My Profile (Graduate View)

public class AlumniMyProfileDto
{
    public string AlumniId { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public AlumniStatus Status { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal WalletBalance { get; set; }
    public int ViewCount { get; set; }
    public List<QualificationHistoryDto> AcademicHistory { get; set; } = new();
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Bio { get; set; }
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? FacebookUrl { get; set; }
    public string? LinkedinUrl { get; set; }
    public List<ContactEmailDto> Emails { get; set; } = new();
    public List<ContactMobileDto> Mobiles { get; set; } = new();
    public List<ContactPhoneDto> Phones { get; set; } = new();
    public AdvisoryWorkflowStatus AdvisoryStatus { get; set; }
    public string? AdvisoryBio { get; set; }
    public int AdvisoryExperienceYears { get; set; }
    public string? AdvisoryRejectionReason { get; set; }
    public List<Guid> ExpertiseIds { get; set; } = new();
    
    // For backward compatibility and Search results
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public bool IsVip { get; set; }
    public List<AlumniEducationDto> Educations { get; set; } = new();
    public List<ExperienceDto> Experiences { get; set; } = new();
}

public class ApplyAsAdvisorDto
{
    public string Bio { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public List<Guid> ExpertiseIds { get; set; } = new();
}

public class WalletActivityDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Type { get; set; } = string.Empty; // "Deposit" | "Payment"
}

public class AdvisoryStatusDto
{
    public AdvisoryWorkflowStatus Status { get; set; }
    public string? Bio { get; set; }
    public int ExperienceYears { get; set; }
    public string? RejectionReason { get; set; }
    public List<Guid> ExpertiseIds { get; set; } = new();
}

public class UpdateMyProfileDto
{
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public byte[]? ProfilePhoto { get; set; }
    public string? Bio { get; set; } 
    public string? JobTitle { get; set; } 
    public string? Company { get; set; }
    public string? FacebookUrl { get; set; }
    public string? LinkedinUrl { get; set; }
    public List<ContactEmailDto> Emails { get; set; } = new();
    public List<ContactMobileDto> Mobiles { get; set; } = new();
    public List<ContactPhoneDto> Phones { get; set; } = new();
}

#endregion

#region Search DTOs (Employee View)

public class AlumniSearchFilterDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public List<int>? GraduationYears { get; set; }
    public List<int>? GraduationSemesters { get; set; }
    public Guid? NationalityId { get; set; }
    public VipFilterOption IsVip { get; set; }
}

public class AlumniListDto : EntityDto<Guid>
{
    public Guid UserId { get; set; }
    public string AlumniId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? Degree { get; set; }
    public string? Major { get; set; }
    public int? GraduationYear { get; set; }
    public int? GraduationSemester { get; set; }
    public string? College { get; set; }
    public bool HasPhoto { get; set; }
    public bool IsVip { get; set; }
    public string? PhotoUrl { get; set; }
    public string? PrimaryEmail { get; set; }
    public decimal GPA { get; set; }
}

public class AlumniProfileDetailDto : AlumniMyProfileDto
{
    // detailed view for admin/employee
}

public class UpdateAlumniPhotoDto
{
    public byte[]? ProfilePhoto { get; set; }
}

#endregion

#region Reporting DTOs

public class AlumniReportInputDto : PagedAndSortedResultRequestDto
{
    public Guid? BranchId { get; set; }
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public List<int>? GraduationYears { get; set; }
    public List<int>? GraduationSemesters { get; set; }
    public AlumniReportType ReportType { get; set; }
}

public class AlumniReportDetailDto
{
    public int SerialNo { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AlumniId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public DateTime? CardExpiryDate { get; set; }
    public int GraduationYear { get; set; }
    public int GraduationSemester { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class AlumniStatisticalReportDto
{
    public List<AlumniReportStatsRowDto> Rows { get; set; } = new();
    public Dictionary<string, int> GrandTotalByCollege { get; set; } = new();
    public int GrandTotal { get; set; }
}

public class AlumniReportStatsRowDto
{
    public int GraduationYear { get; set; }
    public int GraduationSemester { get; set; }
    public Dictionary<string, int> CollegeCounts { get; set; } = new();
    public int RowTotal { get; set; }
}

public class ExpectedGraduatesReportInputDto : PagedAndSortedResultRequestDto
{
    public Guid BranchId { get; set; }
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public decimal? GpaFrom { get; set; }
    public decimal? GpaTo { get; set; }
    public int? PassedHoursFrom { get; set; }
    public int? PassedHoursTo { get; set; }
}

public class ExpectedGraduatesReportOutputDto
{
    public int SerialNo { get; set; }
    public string AlumniId { get; set; } = string.Empty;
    public string AlumniName { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public int PassedHours { get; set; }
    public decimal GPA { get; set; }
}

public class ExpectedGraduateFilterDto : PagedAndSortedResultRequestDto
{
    public Guid? BranchId { get; set; }
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public Guid? MinorId { get; set; }
    public decimal? GpaFrom { get; set; }
    public decimal? GpaTo { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    public string? IdentityType { get; set; }
    public string? IdentityNumber { get; set; }
    public string? StudentId { get; set; }
    public string? StudentName { get; set; }
}

public class ExpectedGraduateDto : EntityDto<string>
{
    public string StudentId { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string CollegeName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public decimal GPA { get; set; }
    public int CreditHoursPassed { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime BirthDate { get; set; }
    public string NationalId { get; set; } = string.Empty;
}

#endregion
