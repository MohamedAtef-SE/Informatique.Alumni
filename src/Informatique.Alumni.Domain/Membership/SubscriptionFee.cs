using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Membership;

public class SubscriptionFee : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public int Year { get; private set; }
    public DateTime SeasonStartDate { get; private set; }
    public DateTime SeasonEndDate { get; private set; }
    public bool IsActive { get; private set; }

    private SubscriptionFee() { }

    public SubscriptionFee(Guid id, string name, decimal amount, int year, DateTime start, DateTime end)
        : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), MembershipConsts.MaxNameLength);
        
        if (amount < 0)
        {
            throw new BusinessException(AlumniDomainErrorCodes.SubscriptionFee.InvalidAmount)
                .WithData("Amount", amount);
        }
        Amount = amount;
        
        if (start >= end)
        {
            throw new BusinessException(AlumniDomainErrorCodes.SubscriptionFee.InvalidDateRange)
                .WithData("StartDate", start)
                .WithData("EndDate", end);
        }
        
        var currentYear = DateTime.UtcNow.Year;
        if (year < currentYear - 10 || year > currentYear + 10)
        {
            throw new BusinessException(AlumniDomainErrorCodes.SubscriptionFee.InvalidYear)
                .WithData("Year", year);
        }
        
        SeasonStartDate = start;
        SeasonEndDate = end;
        Year = year;
        IsActive = true;
    }

    public void Update(string name, decimal amount, DateTime start, DateTime end)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), MembershipConsts.MaxNameLength);
        
        if (amount < 0)
        {
            throw new BusinessException(AlumniDomainErrorCodes.SubscriptionFee.InvalidAmount)
                .WithData("Amount", amount);
        }
        Amount = amount;
        
        if (start >= end)
        {
            throw new BusinessException(AlumniDomainErrorCodes.SubscriptionFee.InvalidDateRange)
                .WithData("StartDate", start)
                .WithData("EndDate", end);
        }
        
        SeasonStartDate = start;
        SeasonEndDate = end;
    }

    public void Activate()
    {
        if (IsActive)
        {
            throw new BusinessException(AlumniDomainErrorCodes.SubscriptionFee.AlreadyActive);
        }
        
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new BusinessException(AlumniDomainErrorCodes.SubscriptionFee.AlreadyInactive);
        }
        
        IsActive = false;
    }

    public void Close()
    {
        if (DateTime.UtcNow < SeasonEndDate)
        {
            throw new BusinessException(AlumniDomainErrorCodes.SubscriptionFee.CannotCloseBeforeEnd)
                .WithData("SeasonEndDate", SeasonEndDate);
        }
        
        IsActive = false;
    }

    public bool IsCurrentlyValid()
    {
        var now = DateTime.UtcNow;
        return IsActive && now >= SeasonStartDate && now <= SeasonEndDate;
    }
}
