using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Benefits;

public class AcademicGrant : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; }
    public string NameEn { get; private set; }
    public string Type { get; private set; } // e.g., "Full", "Partial", "Masters", etc.
    public double Percentage { get; private set; } // 0-100

    private AcademicGrant() { }

    public AcademicGrant(Guid id, string nameAr, string nameEn, string type, double percentage)
        : base(id)
    {
        SetName(nameAr, nameEn);
        SetType(type);
        SetPercentage(percentage);
    }

    public void SetName(string nameAr, string nameEn)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr));
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn));
    }

    public void SetType(string type)
    {
        Type = Check.NotNullOrWhiteSpace(type, nameof(type));
    }

    public void SetPercentage(double percentage)
    {
        if (percentage < 0 || percentage > 100)
        {
            throw new UserFriendlyException("Percentage must be between 0 and 100.");
        }
        Percentage = percentage;
    }
}
