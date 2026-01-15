using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Benefits;

public class CommercialDiscount : FullAuditedAggregateRoot<Guid>
{
    public string ProviderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public string? PromoCode { get; set; }
    public DateTime ValidUntil { get; set; }

    public Guid CategoryId { get; private set; } // [New] Link to Category

    private CommercialDiscount() { }

    public CommercialDiscount(
        Guid id, 
        Guid categoryId,
        string providerName, 
        string title, 
        string description, 
        decimal discountPercentage, 
        string? promoCode, 
        DateTime validUntil)
        : base(id)
    {
        CategoryId = categoryId;
        ProviderName = providerName;
        Title = title;
        Description = description;
        DiscountPercentage = discountPercentage;
        PromoCode = promoCode;
        ValidUntil = validUntil;
    }
}
