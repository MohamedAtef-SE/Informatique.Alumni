
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Syndicates;

public class SyndicateRequirement : FullAuditedAggregateRoot<Guid>
{
    public string CollegeName { get; private set; }
    public string Description { get; private set; }
    public string RequirementsContent { get; private set; } // Stores HTML/Rich Text

    private SyndicateRequirement()
    {
        // Parameterless constructor for ORM
        CollegeName = null!;
        Description = null!;
        RequirementsContent = null!;
    }

    public SyndicateRequirement(Guid id, string collegeName, string description, string requirementsContent)
        : base(id)
    {
        CollegeName = Check.NotNullOrWhiteSpace(collegeName, nameof(collegeName));
        Description = Check.NotNullOrWhiteSpace(description, nameof(description));
        RequirementsContent = Check.NotNullOrWhiteSpace(requirementsContent, nameof(requirementsContent));
    }

    public void Update(string collegeName, string description, string requirementsContent)
    {
        CollegeName = Check.NotNullOrWhiteSpace(collegeName, nameof(collegeName));
        Description = Check.NotNullOrWhiteSpace(description, nameof(description));
        RequirementsContent = Check.NotNullOrWhiteSpace(requirementsContent, nameof(requirementsContent));
    }
}
