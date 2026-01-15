
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace Informatique.Alumni.Benefits;

public class DiscountCategory : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; }
    public string NameEn { get; private set; }
    public string? LogoPath { get; private set; } // Blob Path

    private DiscountCategory()
    {
        NameAr = string.Empty;
        NameEn = string.Empty;
    }

    public DiscountCategory(Guid id, string nameAr, string nameEn, string? logoPath)
        : base(id)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr));
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn));
        LogoPath = logoPath;
    }

    public void UpdateInfo(string nameAr, string nameEn, string? logoPath)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr));
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn));
        LogoPath = logoPath;
    }
}
