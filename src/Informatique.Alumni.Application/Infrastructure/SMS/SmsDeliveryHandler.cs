using System;
using System.Threading.Tasks;
using Informatique.Alumni.Infrastructure.SMS;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;

namespace Informatique.Alumni.Infrastructure.SMS;

public class SmsDeliveryHandler : IDistributedEventHandler<SmsSentEto>, ITransientDependency
{
    private readonly IRepository<SmsDeliveryLog, Guid> _logRepository;

    public SmsDeliveryHandler(IRepository<SmsDeliveryLog, Guid> logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task HandleEventAsync(SmsSentEto eventData)
    {
        var log = new SmsDeliveryLog(
            Guid.NewGuid(),
            eventData.Recipient,
            eventData.Message,
            eventData.Provider,
            eventData.IsSuccess,
            eventData.ErrorMessage
        );

        // In a real scenario, use a background worker if high volume, 
        // but for now direct repository insert is fine for decoupled Event Handler
        await _logRepository.InsertAsync(log);
    }
}
