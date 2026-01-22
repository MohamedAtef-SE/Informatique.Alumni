using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.EmploymentFair;

public class CvSearchFilterDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? AlumniId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CollegeId { get; set; }
    public Guid? MajorId { get; set; }
    public int? GraduationYear { get; set; }
    public double? MinGpa { get; set; }
    public double? MaxGpa { get; set; }
    public string? MilitaryStatus { get; set; } // Consider Enum if available in other modules
    public string? MaritalStatus { get; set; }   // Consider Enum if available
    public bool? IsLookingForJob { get; set; }
}
