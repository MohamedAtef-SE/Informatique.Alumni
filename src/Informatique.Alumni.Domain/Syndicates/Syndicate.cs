using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Syndicates;

public class Syndicate : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Requirements { get; private set; } = string.Empty; // Comma separated list of required doc names eg: "ID,License,GraduationCertificate"

    public decimal Fee { get; private set; }

    private Syndicate() { }

    public Syndicate(Guid id, string name, string description, string requirements, decimal fee)
        : base(id)
    {
        SetName(name);
        SetDescription(description);
        SetRequirements(requirements);
        SetFee(fee);
    }
    
    public void Update(string name, string description, string requirements, decimal fee)
    {
        SetName(name);
        SetDescription(description);
        SetRequirements(requirements);
        SetFee(fee);
    }

    private void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), SyndicateConsts.MaxNameLength);
    }

    private void SetDescription(string description)
    {
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), SyndicateConsts.MaxDescriptionLength);
    }

    private void SetRequirements(string requirements)
    {
         Requirements = Check.NotNull(requirements, nameof(requirements));
    }

    private void SetFee(decimal fee)
    {
        Fee = fee >= 0 ? fee : 0;
    }
}
