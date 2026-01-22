using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.EmploymentFair;

public class AlumniWorkStatus : AuditedEntity<Guid>
{
    public Guid AlumniId { get; private set; }
    public CvWorkStatus Status { get; private set; }

    protected AlumniWorkStatus() { }

    public AlumniWorkStatus(Guid id, Guid alumniId, CvWorkStatus status)
        : base(id)
    {
        AlumniId = alumniId;
        Status = status;
    }

    public void UpdateStatus(CvWorkStatus status)
    {
        Status = status;
    }
}
