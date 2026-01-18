using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Informatique.Alumni.Events;

public class SubscriptionCancelledEventHandler : ILocalEventHandler<SubscriptionCancelledEto>, ITransientDependency
{
    private readonly ILogger<SubscriptionCancelledEventHandler> _logger;

    public SubscriptionCancelledEventHandler(ILogger<SubscriptionCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleEventAsync(SubscriptionCancelledEto eventData)
    {
        // Mocking Email/SMS Notification
        _logger.LogInformation($"[Notification Sent] To Alumni {eventData.AlumniId}: Your subscription for Event {eventData.EventId} has been cancelled. Reason: {eventData.CancellationReason}. Refund Status: {(eventData.IsRefunded ? "Refunded to Wallet" : "Manual Refund Pending")}");
        await Task.CompletedTask;
    }
}
