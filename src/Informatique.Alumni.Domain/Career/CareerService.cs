using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace Informatique.Alumni.Career;

public class CareerService : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string MapUrl { get; private set; } = string.Empty;
    
    public bool HasFees { get; private set; }
    public decimal FeeAmount { get; private set; }
    public DateTime LastSubscriptionDate { get; private set; }

    public Guid ServiceTypeId { get; private set; }
    public Guid BranchId { get; private set; }
    
    public CareerServiceType ServiceType { get; set; }
    public ICollection<CareerServiceTimeslot> Timeslots { get; private set; }

    private CareerService() { }

    public CareerService(
        Guid id, 
        string nameAr, 
        string nameEn, 
        string code,
        string description,
        bool hasFees,
        decimal feeAmount,
        DateTime lastSubscriptionDate,
        Guid serviceTypeId,
        Guid branchId)
        : base(id)
    {
        SetNames(nameAr, nameEn);
        Code = Check.NotNullOrWhiteSpace(code, nameof(code));
        Description = description;
        SetFinancials(hasFees, feeAmount);
        
        LastSubscriptionDate = lastSubscriptionDate;
        ServiceTypeId = serviceTypeId;
        BranchId = branchId;

        Timeslots = new List<CareerServiceTimeslot>();
    }

    public void SetNames(string nameAr, string nameEn)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr), 128);
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn), 128);
    }

    public void SetFinancials(bool hasFees, decimal feeAmount)
    {
        HasFees = hasFees;
        if (HasFees && feeAmount <= 0)
        {
            throw new BusinessException("Career:FeeAmountMustBePositive");
        }
        if (!HasFees && feeAmount != 0)
        {
            throw new BusinessException("Career:FeeAmountMustBeZero");
        }
        FeeAmount = feeAmount;
    }

    public void SetMapUrl(string mapUrl)
    {
        MapUrl = mapUrl;
    }
}
