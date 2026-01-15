using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Career;

public class Job : FullAuditedAggregateRoot<Guid>
{
    public Guid CompanyId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? Requirements { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ClosingDate { get; private set; }

    private Job() { }

    public Job(Guid id, Guid companyId, string title, string description) : base(id)
    {
        CompanyId = Check.NotDefaultOrNull<Guid>(companyId, nameof(companyId));
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), JobConsts.MaxTitleLength);
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), JobConsts.MaxDescriptionLength);
        IsActive = true;
    }

    public void Update(string title, string description, string? requirements = null, DateTime? closingDate = null)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), JobConsts.MaxTitleLength);
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), JobConsts.MaxDescriptionLength);
        
        if (requirements != null)
        {
            Requirements = Check.Length(requirements, nameof(requirements), JobConsts.MaxRequirementsLength);
        }
        
        if (closingDate.HasValue)
        {
            SetClosingDate(closingDate.Value);
        }
    }

    public void SetClosingDate(DateTime closingDate)
    {
        if (closingDate < DateTime.UtcNow)
        {
            throw new BusinessException(AlumniDomainErrorCodes.Job.InvalidClosingDate)
                .WithData("ClosingDate", closingDate);
        }
        
        ClosingDate = closingDate;
    }

    public void UpdateRequirements(string? requirements)
    {
        if (requirements != null)
        {
            Requirements = Check.Length(requirements, nameof(requirements), JobConsts.MaxRequirementsLength);
        }
        else
        {
            Requirements = null;
        }
    }

    public void Close()
    {
        if (!IsActive)
        {
            throw new BusinessException(AlumniDomainErrorCodes.Job.AlreadyClosed);
        }
        
        IsActive = false;
    }

    public void Reopen()
    {
        if (IsActive)
        {
            throw new BusinessException(AlumniDomainErrorCodes.Job.AlreadyActive);
        }
        
        if (ClosingDate.HasValue && ClosingDate.Value < DateTime.UtcNow)
        {
            throw new BusinessException(AlumniDomainErrorCodes.Job.CannotReopenExpired);
        }
        
        IsActive = true;
    }
}
