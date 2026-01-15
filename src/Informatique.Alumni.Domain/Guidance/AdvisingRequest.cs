using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Guidance;

public class AdvisingRequest : FullAuditedAggregateRoot<Guid>
{
    public Guid AlumniId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid AdvisorId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AdvisingRequestStatus Status { get; set; }
    public string? AdminNotes { get; set; }

    private AdvisingRequest() { }

    public AdvisingRequest(Guid id, Guid alumniId, Guid branchId, Guid advisorId, DateTime startTime, DateTime endTime, string subject)
        : base(id)
    {
        AlumniId = alumniId;
        BranchId = branchId;
        AdvisorId = advisorId;
        StartTime = startTime;
        EndTime = endTime;
        Subject = subject;
        Status = AdvisingRequestStatus.Requested;
    }

    public void SetStatus(AdvisingRequestStatus status)
    {
        Status = status;
    }
}
