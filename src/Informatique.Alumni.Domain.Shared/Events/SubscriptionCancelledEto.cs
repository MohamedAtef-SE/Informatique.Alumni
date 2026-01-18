using System;

namespace Informatique.Alumni.Events;

public class SubscriptionCancelledEto
{
    public Guid RegistrationId { get; set; }
    public Guid AlumniId { get; set; }
    public Guid EventId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
    public bool IsRefunded { get; set; }
}
