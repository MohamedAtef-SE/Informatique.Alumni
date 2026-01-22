using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Payment;

public class PaymentMethod : Entity<Guid>
{
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    protected PaymentMethod() { }

    public PaymentMethod(Guid id, string name, bool isActive = true)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        IsActive = isActive;
    }

    public void ToggleStatus(bool isActive)
    {
        IsActive = isActive;
    }
}
