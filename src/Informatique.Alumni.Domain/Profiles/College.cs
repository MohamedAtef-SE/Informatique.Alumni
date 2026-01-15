using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace Informatique.Alumni.Profiles;

public class College : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;

    private College() { }

    public College(Guid id, string name)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128);
    }
}
