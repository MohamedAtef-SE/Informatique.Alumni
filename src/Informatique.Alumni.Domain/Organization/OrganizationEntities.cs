using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Organization;

public class Department : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid CollegeId { get; private set; }
    public string? ExternalId { get; set; }

    public virtual ICollection<Specialization> Specializations { get; private set; }

    private Department() { }

    public Department(Guid id, string name, Guid collegeId, string? externalId = null)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        CollegeId = collegeId;
        ExternalId = externalId;
        Specializations = new List<Specialization>();
    }
}

public class Specialization : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid DepartmentId { get; private set; }
    public SpecializationType Type { get; private set; }
    public string? ExternalId { get; set; }

    private Specialization() { }

    public Specialization(Guid id, string name, Guid departmentId, SpecializationType type, string? externalId = null)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        DepartmentId = departmentId;
        Type = type;
        ExternalId = externalId;
    }
}

public class AcademicLevel : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string? ExternalId { get; set; }

    private AcademicLevel() { }

    public AcademicLevel(Guid id, string name, string? externalId = null)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        ExternalId = externalId;
    }
}
