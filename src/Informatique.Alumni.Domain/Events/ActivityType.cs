using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Events;

public class ActivityType : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private ActivityType() { }

    public ActivityType(Guid id, string nameAr, string nameEn, bool isActive = true) : base(id)
    {
        SetNameAr(nameAr);
        SetNameEn(nameEn);
        IsActive = isActive;
    }

    public void SetNameAr(string nameAr)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr), EventConsts.MaxTitleLength);
    }

    public void SetNameEn(string nameEn)
    {
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn), EventConsts.MaxTitleLength);
    }
    
    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
