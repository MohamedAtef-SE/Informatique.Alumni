using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace Informatique.Alumni.Guidance;

public class GuidanceEventHandler : IDistributedEventHandler<SessionRequestedEto>, ITransientDependency
{
    private readonly ILogger<GuidanceEventHandler> _logger;

    public GuidanceEventHandler(ILogger<GuidanceEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(SessionRequestedEto eventData)
    {
        // In a real system, you would look up the Advisor's email and send it via IEmailSender
        _logger.LogInformation($"[NOTIFICATION] Advisor {eventData.AdvisorId}: New guidance session requested by Alumni {eventData.AlumniId} for {eventData.RequestedTime}");
        
        return Task.CompletedTask;
    }
}
