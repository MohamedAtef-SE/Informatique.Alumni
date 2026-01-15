using System;

namespace Informatique.Alumni.Infrastructure.SMS;

public class SmsSentEto
{
    public string Recipient { get; set; }
    public string Message { get; set; }
    public string Provider { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentTime { get; set; }

    public SmsSentEto(string recipient, string message, string provider, bool isSuccess, string? errorMessage = null)
    {
        Recipient = recipient;
        Message = message;
        Provider = provider;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        SentTime = DateTime.Now;
    }
}
