using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Magazine;

public class ArticleCategory : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    private ArticleCategory() { }

    public ArticleCategory(Guid id, string nameEn, string nameAr) : base(id)
    {
        NameEn = nameEn;
        NameAr = nameAr;
    }
}

