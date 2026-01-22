using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.EmploymentFair;

public class CvAchievement : Entity<Guid>
{
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }

    public CvAchievement(Guid id) : base(id) { }
}
