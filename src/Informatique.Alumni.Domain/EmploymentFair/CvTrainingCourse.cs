using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.EmploymentFair;

public class CvTrainingCourse : Entity<Guid>
{
    public string Name { get; set; }
    public string CenterName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public CvTrainingCourse(Guid id) : base(id) { }
}
