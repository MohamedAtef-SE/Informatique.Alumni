using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Benefits;

public class DiscountCategory : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; }
    public string NameEn { get; private set; }
    public string? LogoUrl { get; private set; }

    private DiscountCategory() { }

    public DiscountCategory(Guid id, string nameAr, string nameEn, string? logoUrl)
        : base(id)
    {
        SetName(nameAr, nameEn);
        LogoUrl = logoUrl;
    }

    public void SetName(string nameAr, string nameEn)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr));
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn));
    }

    public void SetLogoUrl(string? logoUrl)
    {
        LogoUrl = logoUrl;
    }
}
