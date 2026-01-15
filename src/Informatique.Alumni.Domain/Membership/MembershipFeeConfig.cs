using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Membership;

public class MembershipFeeConfig : AuditedAggregateRoot<Guid>
{
    public decimal OneYearFee { get; private set; }
    public decimal TwoYearsFee { get; private set; }
    public decimal MoreThanTwoYearsFee { get; private set; }

    private MembershipFeeConfig() { }

    public MembershipFeeConfig(Guid id, decimal oneYearFee, decimal twoYearsFee, decimal moreThanTwoYearsFee)
        : base(id)
    {
        SetFees(oneYearFee, twoYearsFee, moreThanTwoYearsFee);
    }

    public void SetFees(decimal oneYearFee, decimal twoYearsFee, decimal moreThanTwoYearsFee)
    {
        if (oneYearFee < 0 || twoYearsFee < 0 || moreThanTwoYearsFee < 0)
        {
            throw new ArgumentException("Fees cannot be negative.");
        }

        OneYearFee = oneYearFee;
        TwoYearsFee = twoYearsFee;
        MoreThanTwoYearsFee = moreThanTwoYearsFee;
    }
}
