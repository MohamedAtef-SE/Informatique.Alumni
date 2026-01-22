using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.EmploymentFair;

public class AvailableJob : FullAuditedAggregateRoot<Guid>
{
    public string JobTitle { get; private set; }
    public string CompanyName { get; private set; }
    public string? CompanyLogo { get; private set; }
    public string Address { get; private set; }
    public string Specialization { get; private set; } // Mapped to Major
    public string BriefDescription { get; private set; }
    public string Requirements { get; private set; }
    public int? AvailableOpportunitiesCount { get; private set; }
    public bool IsActive { get; private set; }

    protected AvailableJob() { }

    public AvailableJob(
        Guid id,
        string jobTitle,
        string companyName,
        string address,
        string specialization,
        string briefDescription,
        string requirements,
        int? availableOpportunitiesCount = null,
        string? companyLogo = null)
        : base(id)
    {
        JobTitle = Check.NotNullOrWhiteSpace(jobTitle, nameof(jobTitle));
        CompanyName = Check.NotNullOrWhiteSpace(companyName, nameof(companyName));
        Address = Check.NotNullOrWhiteSpace(address, nameof(address));
        Specialization = Check.NotNullOrWhiteSpace(specialization, nameof(specialization));
        BriefDescription = Check.NotNullOrWhiteSpace(briefDescription, nameof(briefDescription));
        Requirements = Check.NotNullOrWhiteSpace(requirements, nameof(requirements));
        AvailableOpportunitiesCount = availableOpportunitiesCount;
        CompanyLogo = companyLogo;
        IsActive = true; // Default to active
    }

    public void Update(
        string jobTitle,
        string companyName,
        string address,
        string specialization,
        string briefDescription,
        string requirements,
        int? availableOpportunitiesCount,
        string? companyLogo)
    {
        JobTitle = Check.NotNullOrWhiteSpace(jobTitle, nameof(jobTitle));
        CompanyName = Check.NotNullOrWhiteSpace(companyName, nameof(companyName));
        Address = Check.NotNullOrWhiteSpace(address, nameof(address));
        Specialization = Check.NotNullOrWhiteSpace(specialization, nameof(specialization));
        BriefDescription = Check.NotNullOrWhiteSpace(briefDescription, nameof(briefDescription));
        Requirements = Check.NotNullOrWhiteSpace(requirements, nameof(requirements));
        AvailableOpportunitiesCount = availableOpportunitiesCount;
        CompanyLogo = companyLogo;
    }

    public void ToggleActive(bool isActive)
    {
        IsActive = isActive;
    }
}
