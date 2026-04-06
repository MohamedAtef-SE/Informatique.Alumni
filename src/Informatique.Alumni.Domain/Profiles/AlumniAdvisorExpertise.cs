using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Profiles;

/// <summary>
/// Junction entity for Many-to-Many relationship between Alumni and their Advisory Expertise areas.
/// Uses dedicated AdvisoryCategory for professional specialization.
/// </summary>
public class AlumniAdvisorExpertise : Entity
{
    public Guid AlumniProfileId { get; private set; }
    public Guid AdvisoryCategoryId { get; private set; }

    private AlumniAdvisorExpertise() { }

    public AlumniAdvisorExpertise(Guid alumniProfileId, Guid advisoryCategoryId)
    {
        AlumniProfileId = alumniProfileId;
        AdvisoryCategoryId = advisoryCategoryId;
    }

    public override object[] GetKeys()
    {
        return new object[] { AlumniProfileId, AdvisoryCategoryId };
    }
}
