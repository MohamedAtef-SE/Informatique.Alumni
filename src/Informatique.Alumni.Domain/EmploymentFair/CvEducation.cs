using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.EmploymentFair;

public class CvEducation : Entity<Guid>
{
    public string InstitutionName { get; set; }
    public string Degree { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Minimal implementation for compilation
    public CvEducation(Guid id) : base(id) { }
}
