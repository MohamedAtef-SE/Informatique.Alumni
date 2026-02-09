using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace Informatique.Alumni.Profiles;

public class College : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid? BranchId { get; set; } // Optional initially or Required? "Branch has many Colleges". Usually required.
    public string? ExternalId { get; set; }

    public virtual System.Collections.Generic.ICollection<Informatique.Alumni.Organization.Department> Departments { get; private set; }

    private College() 
    {
        Departments = new System.Collections.Generic.List<Informatique.Alumni.Organization.Department>();
    }

    public College(Guid id, string name, Guid? branchId = null, string? externalId = null)
        : base(id)
    {
        BranchId = branchId;
        ExternalId = externalId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128);
    }
}
