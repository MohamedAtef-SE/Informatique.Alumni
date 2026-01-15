
using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace Informatique.Alumni.Benefits;

public class AcademicDiscount : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; }
    public string NameEn { get; private set; }
    public AcademicDiscountType Type { get; private set; }
    public decimal Percentage { get; private set; }

    private AcademicDiscount() 
    {
        NameAr = string.Empty;
        NameEn = string.Empty;
    }

    public AcademicDiscount(Guid id, string nameAr, string nameEn, AcademicDiscountType type, decimal percentage)
        : base(id)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr));
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn));
        Type = type;
        SetPercentage(percentage);
    }
    
    public void UpdateInfo(string nameAr, string nameEn, AcademicDiscountType type, decimal percentage)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr));
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn));
        Type = type;
        SetPercentage(percentage);
    }

    private void SetPercentage(decimal percentage)
    {
        if (percentage <= 0 || percentage > 100)
        {
            throw new UserFriendlyException("Percentage must be between 0 and 100.");
        }
        Percentage = percentage;
    }
}
