using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Infrastructure.SMS;

public class SmsDeliveryLog : Entity<Guid>
{
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }

    private SmsDeliveryLog() { }

    public SmsDeliveryLog(Guid id, string recipient, string message, string provider, bool isSuccess, string? errorMessage = null)
        : base(id)
    {
        Recipient = recipient;
        Message = message;
        Provider = provider;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Timestamp = DateTime.Now;
    }
}
