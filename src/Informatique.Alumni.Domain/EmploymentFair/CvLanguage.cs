using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.EmploymentFair;

public class CvLanguage : Entity<Guid>
{
    public string Name { get; set; }
    public string ProficiencyLevel { get; set; } // Beginner -> Native

    public CvLanguage(Guid id) : base(id) { }
}
