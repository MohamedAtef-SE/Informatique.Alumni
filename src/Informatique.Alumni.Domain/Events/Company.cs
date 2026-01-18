using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Events;

public class Company : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string LogoBlobName { get; private set; } = string.Empty;
    public string? WebsiteUrl { get; private set; }

    private Company() { }

    public Company(Guid id, string nameAr, string nameEn, string logoBlobName, string? websiteUrl = null)
        : base(id)
    {
        SetNameAr(nameAr);
        SetNameEn(nameEn);
        SetLogoBlobName(logoBlobName);
        WebsiteUrl = websiteUrl;
    }

    public void SetNameAr(string nameAr)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr));
    }

    public void SetNameEn(string nameEn)
    {
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn));
    }

    public void SetLogoBlobName(string logoBlobName)
    {
        LogoBlobName = Check.NotNullOrWhiteSpace(logoBlobName, nameof(logoBlobName));
    }

    public void SetWebsiteUrl(string? websiteUrl)
    {
        WebsiteUrl = websiteUrl;
    }
}
