using System;

namespace Informatique.Alumni.Career;

[Serializable]
public class CareerSubscriptionCancelledEto
{
    public Guid SubscriptionId { get; set; }
    public Guid AlumniId { get; set; }
    public Guid CareerServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string CancellationReason { get; set; } = string.Empty;
    public bool WasRefunded { get; set; }
    public decimal RefundAmount { get; set; }
}
