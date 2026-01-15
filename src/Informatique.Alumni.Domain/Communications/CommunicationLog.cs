using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Communications;

/// <summary>
/// Tracks every message sent from the system (Email/SMS).
/// </summary>
public class CommunicationLog : CreationAuditedEntity<Guid>
{
    public Guid SenderId { get; set; } // EmployeeId
    public Guid? RecipientId { get; set; } // AlumniId (Nullable in case of system message or failure to resolve)
    public string Channel { get; set; } // Email / SMS
    public string Subject { get; set; } // Subject for Email, or "SMS" for SMS
    public string Content { get; set; } // Body
    public string Status { get; set; } // Success / Failed
    public string? ErrorMessage { get; set; }

    protected CommunicationLog() { }

    public CommunicationLog(Guid id, Guid senderId, Guid? recipientId, string channel, string subject, string content, string status)
        : base(id)
    {
        SenderId = senderId;
        RecipientId = recipientId;
        Channel = channel;
        Subject = subject;
        Content = content;
        Status = status;
    }
}
