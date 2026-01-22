using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.EmploymentFair;

public class JobApplication : FullAuditedAggregateRoot<Guid>
{
    public Guid JobId { get; private set; }
    public Guid AlumniId { get; private set; }
    public DateTime ApplicationDate { get; private set; }
    public string CvFileUrl { get; private set; }

    private JobApplication()
    {
        /* Required by EF Core */
    }

    public JobApplication(
        Guid id,
        Guid jobId,
        Guid alumniId,
        string cvFileUrl,
        DateTime applicationDate)
        : base(id)
    {
        JobId = jobId;
        AlumniId = alumniId;
        SetCvFileUrl(cvFileUrl);
        ApplicationDate = applicationDate;
    }

    public void SetCvFileUrl(string cvFileUrl)
    {
        CvFileUrl = Check.NotNullOrWhiteSpace(cvFileUrl, nameof(cvFileUrl));
    }
}
