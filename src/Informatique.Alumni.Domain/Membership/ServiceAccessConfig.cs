using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Membership;

public class ServiceAccessConfig : FullAuditedAggregateRoot<Guid>
{
    public string ServiceName { get; private set; } = string.Empty;
    public AccessPolicy AccessPolicy { get; private set; }

    private ServiceAccessConfig() { }

    public ServiceAccessConfig(Guid id, string serviceName, AccessPolicy accessPolicy = AccessPolicy.ActiveMembersOnly)
        : base(id)
    {
        ServiceName = Check.NotNullOrWhiteSpace(serviceName, nameof(serviceName));
        AccessPolicy = accessPolicy;
    }

    public void SetPolicy(AccessPolicy policy)
    {
        AccessPolicy = policy;
    }
}
