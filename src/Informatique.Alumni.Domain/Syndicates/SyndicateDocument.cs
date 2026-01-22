using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Syndicates;

public class SyndicateDocument : Entity<Guid>
{
    public Guid SyndicateSubscriptionId { get; private set; }
    public string RequirementName { get; private set; } = string.Empty;
    public string FileBlobName { get; private set; } = string.Empty;

    private SyndicateDocument() { }

    public SyndicateDocument(Guid id, Guid syndicateSubscriptionId, string requirementName, string fileBlobName)
        : base(id)
    {
        SyndicateSubscriptionId = syndicateSubscriptionId;
        RequirementName = Check.NotNullOrWhiteSpace(requirementName, nameof(requirementName));
        FileBlobName = Check.NotNullOrWhiteSpace(fileBlobName, nameof(fileBlobName));
    }
}
