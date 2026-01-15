
using System;
using Volo.Abp.EventBus;

namespace Informatique.Alumni.Guidance;

[EventName("Informatique.Alumni.Guidance.AdvisingRequestStatusChanged")]
public class AdvisingRequestStatusChangedEto
{
    public Guid RequestId { get; set; }
    public Guid AlumniId { get; set; }
    public AdvisingRequestStatus OldStatus { get; set; }
    public AdvisingRequestStatus NewStatus { get; set; }
}

[EventName("Informatique.Alumni.Guidance.SessionRequested")]
public class SessionRequestedEto
{
    public Guid AdvisorId { get; set; }
    public Guid AlumniId { get; set; }
    public Guid AdvisingRequestId { get; set; }
    public DateTime RequestedTime { get; set; }
}
