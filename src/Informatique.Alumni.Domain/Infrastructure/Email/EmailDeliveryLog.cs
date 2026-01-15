using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Infrastructure.Email;

public class EmailDeliveryLog : Entity<Guid>
{
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }

    private EmailDeliveryLog() { }

    public EmailDeliveryLog(Guid id, string recipient, string subject, bool isSuccess, string? errorMessage = null)
        : base(id)
    {
        Recipient = recipient;
        Subject = subject;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Timestamp = DateTime.Now;
    }
}
