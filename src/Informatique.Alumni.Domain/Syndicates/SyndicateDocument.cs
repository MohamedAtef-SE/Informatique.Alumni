using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Syndicates;

public class SyndicateDocument : Entity<Guid>
{
    public Guid SyndicateSubscriptionId { get; set; }
    public string RequirementName { get; set; } = string.Empty;
    public string FileBlobName { get; set; } = string.Empty;

    private SyndicateDocument() { }

    public SyndicateDocument(Guid id, Guid syndicateSubscriptionId, string requirementName, string fileBlobName)
        : base(id)
    {
        SyndicateSubscriptionId = syndicateSubscriptionId;
        RequirementName = requirementName;
        FileBlobName = fileBlobName;
    }
}
