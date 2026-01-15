using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Certificates;

/// <summary>
/// Audit trail entity for tracking all status changes on certificate requests.
/// Business Rule: Every status change must be logged with Who and When.
/// </summary>
public class CertificateRequestHistory : Entity<Guid>
{
    public Guid CertificateRequestId { get; private set; }
    public CertificateRequestStatus OldStatus { get; private set; }
    public CertificateRequestStatus NewStatus { get; private set; }
    public string? Note { get; private set; }
    public Guid ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }

    private CertificateRequestHistory() { }

    public CertificateRequestHistory(
        Guid id,
        Guid certificateRequestId,
        CertificateRequestStatus oldStatus,
        CertificateRequestStatus newStatus,
        Guid changedByUserId,
        string? note = null)
        : base(id)
    {
        CertificateRequestId = certificateRequestId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedByUserId = changedByUserId;
        Note = note;
        ChangedAt = DateTime.UtcNow;
    }
}
