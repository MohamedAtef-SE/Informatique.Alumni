using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.EmploymentFair;

public class CvWorkExperience : Entity<Guid>
{
    public string JobTitle { get; private set; }
    public string CompanyName { get; private set; }
    public bool IsCurrent { get; internal set; } // Internal set to allow logic in AggregateRoot/Manager
    public JobType JobType { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string? Description { get; private set; }

    private CvWorkExperience()
    {
        /* Required by EF Core */
    }

    public CvWorkExperience(
        Guid id,
        string jobTitle,
        string companyName,
        JobType jobType,
        DateTime startDate,
        DateTime? endDate = null,
        bool isCurrent = false,
        string? description = null)
        : base(id)
    {
        SetJobTitle(jobTitle);
        SetCompanyName(companyName);
        JobType = jobType;
        SetDates(startDate, endDate);
        IsCurrent = isCurrent;
        Description = description;
    }

    public void SetJobTitle(string jobTitle)
    {
        JobTitle = Check.NotNullOrWhiteSpace(jobTitle, nameof(jobTitle), maxLength: 256);
    }

    public void SetCompanyName(string companyName)
    {
        CompanyName = Check.NotNullOrWhiteSpace(companyName, nameof(companyName), maxLength: 256);
    }

    public void SetJobType(JobType jobType)
    {
        JobType = jobType;
    }

    public void SetDates(DateTime startDate, DateTime? endDate)
    {
        if (endDate.HasValue && endDate.Value < startDate)
        {
            throw new BusinessException("EmploymentFair:EndDateMustBeGreaterThanStartDate");
        }
        
        // Future date validation
        if (startDate > DateTime.Now)
        {
             throw new BusinessException("EmploymentFair:StartDateCannotBeInFuture");
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    public void SetDescription(string? description)
    {
        Description = description; 
    }

    public void Update(
        string jobTitle,
        string companyName,
        JobType jobType,
        DateTime startDate,
        DateTime? endDate,
        bool isCurrent,
        string? description)
    {
        SetJobTitle(jobTitle);
        SetCompanyName(companyName);
        SetJobType(jobType);
        SetDates(startDate, endDate);
        IsCurrent = isCurrent;
        SetDescription(description);
    }
}
