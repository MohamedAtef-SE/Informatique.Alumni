using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Health;

public class MedicalCategory : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public MedicalPartnerType BaseType { get; set; }
    public bool IsActive { get; set; } = true;

    private MedicalCategory() 
    {
        NameEn = string.Empty;
        NameAr = string.Empty;
    }

    public MedicalCategory(Guid id, string nameEn, string nameAr, MedicalPartnerType baseType) : base(id)
    {
        NameEn = nameEn;
        NameAr = nameAr;
        BaseType = baseType;
    }
}
