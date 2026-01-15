using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace Informatique.Alumni.Profiles;

public class Major : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public Guid CollegeId { get; private set; }

    private Major() { }

    public Major(Guid id, Guid collegeId, string name)
        : base(id)
    {
        CollegeId = collegeId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128);
    }
}
