using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace Informatique.Alumni.Guidance;

public class GuidanceEventHandler : 
    IDistributedEventHandler<SessionRequestedEto>, 
    IDistributedEventHandler<AdvisoryApplicationStatusChangedEto>,
    ITransientDependency
{
    private readonly ILogger<GuidanceEventHandler> _logger;

    public GuidanceEventHandler(ILogger<GuidanceEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(SessionRequestedEto eventData)
    {
        _logger.LogInformation($"[NOTIFICATION] Advisor {eventData.AdvisorId}: New guidance session requested by Alumni {eventData.AlumniId} for {eventData.RequestedTime}");
        return Task.CompletedTask;
    }

    public Task HandleEventAsync(AdvisoryApplicationStatusChangedEto eventData)
    {
        var message = eventData.Status == AdvisoryWorkflowStatus.Approved
            ? "Congratulations! Your application to be an Advisor has been approved."
            : $"Your Advisor application was rejected. Reason: {eventData.RejectionReason}";

        _logger.LogInformation($"[NOTIFICATION] Alumni {eventData.AlumniId}: {message}");
        
        // In a real system:
        // await _emailSender.SendAsync(alumniEmail, "Advisor Status Update", message);
        
        return Task.CompletedTask;
    }
}
