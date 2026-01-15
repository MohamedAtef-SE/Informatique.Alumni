using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace Informatique.Alumni.Career;

public class CareerServiceType : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private CareerServiceType() { }

    public CareerServiceType(Guid id, string nameAr, string nameEn, bool isActive = true)
        : base(id)
    {
        SetNames(nameAr, nameEn);
        IsActive = isActive;
    }

    public void SetNames(string nameAr, string nameEn)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr), 128);
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn), 128);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
