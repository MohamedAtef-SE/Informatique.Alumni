using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Membership;

public class RequestStatusHistory : CreationAuditedEntity<Guid>
{
    public Guid AssociationRequestId { get; private set; }
    public MembershipRequestStatus Status { get; private set; }
    public string? Note { get; private set; }

    private RequestStatusHistory() { }

    internal RequestStatusHistory(Guid id, Guid requestId, MembershipRequestStatus status, string? note = null)
        : base(id)
    {
        AssociationRequestId = requestId;
        Status = status;
        Note = note;
    }
}
