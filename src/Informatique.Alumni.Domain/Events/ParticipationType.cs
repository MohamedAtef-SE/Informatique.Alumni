using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Events;

public class ParticipationType : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;

    private ParticipationType() { }

    public ParticipationType(Guid id, string nameAr, string nameEn)
        : base(id)
    {
        SetNameAr(nameAr);
        SetNameEn(nameEn);
    }

    public void SetNameAr(string nameAr)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr));
    }

    public void SetNameEn(string nameEn)
    {
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn));
    }
}
