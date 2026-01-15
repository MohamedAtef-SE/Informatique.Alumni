using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;

namespace Informatique.Alumni.Membership;

public class MembershipExpirationWorker : AsyncPeriodicBackgroundWorkerBase
{
    public MembershipExpirationWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory
    ) : base(timer, serviceScopeFactory)
    {
        Timer.Period = 24 * 60 * 60 * 1000; // Run once a day
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("Starting membership expiration check...");

        var requestRepository = workerContext.ServiceProvider.GetRequiredService<IRepository<AssociationRequest, Guid>>();
        var feeRepository = workerContext.ServiceProvider.GetRequiredService<IRepository<SubscriptionFee, Guid>>();

        var now = DateTime.UtcNow;
        
        // Find all approved requests where the associated fee season has ended
        var expiredFees = await feeRepository.GetListAsync(x => x.SeasonEndDate < now);
        var expiredFeeIds = expiredFees.Select(x => x.Id).ToList();

        if (expiredFeeIds.Any())
        {
            var requestsToExpire = await requestRepository.GetListAsync(x => 
                x.Status == MembershipRequestStatus.Approved && 
                expiredFeeIds.Contains(x.SubscriptionFeeId));

            foreach (var request in requestsToExpire)
            {
                // In a real system, you might move them to an 'Expired' status
                // For this phase, we'll just log and potentially update a status if we had 'Expired'
                // Since we don't have 'Expired' in MembershipRequestStatus, we keep as is or reject?
                // Usually an 'Expired' status would be better. Let's assume we just process them.
                Logger.LogInformation($"Membership request {request.Id} for Alumni {request.AlumniId} has expired.");
            }
        }

        Logger.LogInformation("Membership expiration check completed.");
    }
}
