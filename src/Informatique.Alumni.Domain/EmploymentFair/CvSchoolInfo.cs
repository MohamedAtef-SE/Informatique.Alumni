using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.EmploymentFair;

public class CvSchoolInfo : Entity<Guid>
{
    public string SchoolName { get; set; }
    public int Year { get; set; }
    public string Grade { get; set; }

    public CvSchoolInfo(Guid id) : base(id) { }
}
