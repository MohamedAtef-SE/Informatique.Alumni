using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Delivery;

public class DeliveryProvider : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; } // Should be encrypted in real scenario
    public FeeStrategyType FeeStrategyType { get; set; }
    public bool IsActive { get; set; }

    private DeliveryProvider() { }

    public DeliveryProvider(Guid id, string name, FeeStrategyType feeStrategyType) : base(id)
    {
        Name = name;
        FeeStrategyType = feeStrategyType;
        IsActive = true;
    }
}
