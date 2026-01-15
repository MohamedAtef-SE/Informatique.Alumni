using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Trips;

/// <summary>
/// Child entity for trip document requirements.
/// DDD: Owned entity, part of AlumniTrip aggregate.
/// Clean Code: Simple, focused entity.
/// </summary>
public class TripRequirement : Entity<Guid>
{
    /// <summary>
    /// Foreign key to the owning trip.
    /// </summary>
    public Guid TripId { get; private set; }
    
    /// <summary>
    /// Foreign key to the master document definition.
    /// Example: Passport, ID Card, etc.
    /// </summary>
    public Guid DocumentId { get; private set; }
    
    /// <summary>
    /// The target audience for this requirement.
    /// Business Rule: Different requirements for members vs companions vs children.
    /// </summary>
    public TripTargetAudience TargetAudience { get; private set; }
    
    /// <summary>
    /// Whether the document should be submitted as original or copy.
    /// </summary>
    public DocumentSubmissionType SubmissionType { get; private set; }

    // EF Core constructor
    private TripRequirement()
    {
    }

    /// <summary>
    /// Constructor for creating a new requirement.
    /// SRP: Simple initialization without complex logic.
    /// </summary>
    public TripRequirement(
        Guid id,
        Guid tripId,
        Guid documentId,
        TripTargetAudience targetAudience,
        DocumentSubmissionType submissionType) : base(id)
    {
        TripId = tripId;
        DocumentId = documentId;
        TargetAudience = targetAudience;
        SubmissionType = submissionType;
    }
}
