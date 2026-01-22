using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.EmploymentFair;

public class CvSkill : Entity<Guid>
{
    public string Name { get; set; }
    public string Proficiency { get; set; }

    public CvSkill(Guid id) : base(id) { }
}
