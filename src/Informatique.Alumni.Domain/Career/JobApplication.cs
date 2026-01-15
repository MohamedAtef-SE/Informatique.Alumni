using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Career;

public class JobApplication : FullAuditedAggregateRoot<Guid>
{
    public Guid JobId { get; private set; }
    public Guid AlumniId { get; private set; }
    public DateTime ApplicationDate { get; private set; }
    public string CvSnapshotBlobName { get; private set; } = string.Empty;

    private JobApplication() { }

    public JobApplication(Guid id, Guid jobId, Guid alumniId, string cvSnapshotBlobName) : base(id)
    {
        JobId = Check.NotDefaultOrNull<Guid>(jobId, nameof(jobId));
        AlumniId = Check.NotDefaultOrNull<Guid>(alumniId, nameof(alumniId));
        CvSnapshotBlobName = Check.NotNullOrWhiteSpace(cvSnapshotBlobName, nameof(cvSnapshotBlobName));
        ApplicationDate = DateTime.UtcNow; // Will be overridden by CreationTime from auditing
    }
}

