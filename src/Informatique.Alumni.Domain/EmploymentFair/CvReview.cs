using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.EmploymentFair;

public class CvReview : FullAuditedAggregateRoot<Guid>
{
    public Guid GraduateId { get; private set; }
    public string OriginalFileUrl { get; private set; }
    public string? ReviewedFileUrl { get; private set; }
    public string? Notes { get; private set; }
    public CvReviewStatus Status { get; private set; }

    protected CvReview() { }

    public CvReview(Guid id, Guid graduateId, string originalFileUrl)
        : base(id)
    {
        GraduateId = graduateId;
        OriginalFileUrl = originalFileUrl;
        Status = CvReviewStatus.Pending;
    }

    public void MarkAsInProgress()
    {
        Status = CvReviewStatus.InProgress;
    }

    public void Complete(string reviewedFileUrl, string notes)
    {
        ReviewedFileUrl = reviewedFileUrl;
        Notes = notes;
        Status = CvReviewStatus.Completed;
    }
}
