using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Health;

public class MedicalOffer : FullAuditedEntity<Guid>
{
    public Guid MedicalPartnerId { get; private set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DiscountCode { get; set; }
    public bool IsActive { get; set; } = true;

    private MedicalOffer() { }

    internal MedicalOffer(Guid id, Guid partnerId, string title, string description, string? discountCode = null)
        : base(id)
    {
        MedicalPartnerId = partnerId;
        Title = title;
        Description = description;
        DiscountCode = discountCode;
    }
}
