using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Companies;

public class Company : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string LogoBlobName { get; private set; } = string.Empty;
    public string? WebsiteUrl { get; private set; }
    public string? Industry { get; private set; }
    public string? Description { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Company() { }

    public Company(
        Guid id, 
        string nameAr, 
        string nameEn, 
        string logoBlobName, 
        string? websiteUrl = null,
        string? industry = null,
        string? description = null,
        string? email = null,
        string? phoneNumber = null,
        bool isActive = true)
        : base(id)
    {
        SetNameAr(nameAr);
        SetNameEn(nameEn);
        SetLogoBlobName(logoBlobName);
        WebsiteUrl = websiteUrl;
        Industry = industry;
        Description = description;
        Email = email;
        PhoneNumber = phoneNumber;
        IsActive = isActive;
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

    public void SetIndustry(string? industry)
    {
        Industry = industry;
    }

    public void SetDescription(string? description)
    {
        Description = description;
    }

    public void SetEmail(string? email)
    {
        Email = email;
    }

    public void SetPhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
