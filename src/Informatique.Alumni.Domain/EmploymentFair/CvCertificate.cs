using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.EmploymentFair;

public class CvCertificate : Entity<Guid>
{
    public string Name { get; set; }
    public string Authority { get; set; }
    public DateTime Date { get; set; }

    public CvCertificate(Guid id) : base(id) { }
}
