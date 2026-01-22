using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Guidance;

public class AdvisingRequest : FullAuditedAggregateRoot<Guid>
{
    public Guid AlumniId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid AdvisorId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string Subject { get; private set; }
    public string? Description { get; set; }
    public AdvisingRequestStatus Status { get; private set; }
    public string? AdminNotes { get; private set; }

    private AdvisingRequest() { }

    public AdvisingRequest(Guid id, Guid alumniId, Guid branchId, Guid advisorId, DateTime startTime, DateTime endTime, string subject)
        : base(id)
    {
        AlumniId = alumniId;
        BranchId = branchId;
        AdvisorId = advisorId;
        StartTime = startTime;
        EndTime = endTime;
        Subject = Check.NotNullOrWhiteSpace(subject, nameof(subject));
        Status = AdvisingRequestStatus.Pending; // Requirement: Pending
    }

    public void Approve()
    {
        EnsureStatusIsPending();
        Status = AdvisingRequestStatus.Approved;
    }

    public void Reject(string reason)
    {
        EnsureStatusIsPending();
        Status = AdvisingRequestStatus.Rejected;
        AdminNotes = Check.NotNullOrWhiteSpace(reason, nameof(reason));
    }

    private void EnsureStatusIsPending()
    {
        if (Status != AdvisingRequestStatus.Pending)
        {
            throw new UserFriendlyException("Cannot change status of a request that is already decided.");
        }
    }
}
