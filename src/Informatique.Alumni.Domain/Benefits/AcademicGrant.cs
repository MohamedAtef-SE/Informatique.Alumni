using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Benefits;

public class AcademicGrant : FullAuditedAggregateRoot<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; // Rich Text
    public decimal Amount { get; set; }
    public DateTime ValidUntil { get; set; }

    private AcademicGrant() { }

    public AcademicGrant(Guid id, string title, string description, decimal amount, DateTime validUntil)
        : base(id)
    {
        Title = title;
        Description = description;
        Amount = amount;
        ValidUntil = validUntil;
    }
}
