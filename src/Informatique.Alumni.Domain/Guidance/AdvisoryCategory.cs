using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Guidance;

/// <summary>
/// Professional categories for Advisor expertise areas (e.g., Software Engineering, Career Pivot, Mock Interviews).
/// These are distinct from general Career Service Types.
/// </summary>
public class AdvisoryCategory : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; }
    public string NameEn { get; private set; }
    public bool IsActive { get; private set; }

    private AdvisoryCategory() 
    {
        NameAr = string.Empty;
        NameEn = string.Empty;
    }

    public AdvisoryCategory(Guid id, string nameAr, string nameEn, bool isActive = true)
        : base(id)
    {
        SetName(nameAr, nameEn);
        IsActive = isActive;
    }

    public void SetName(string nameAr, string nameEn)
    {
        NameAr = Volo.Abp.Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr));
        NameEn = Volo.Abp.Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn));
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
