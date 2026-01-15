using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Infrastructure.Email;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;

namespace Informatique.Alumni.Infrastructure.Email;

public class EmailLogCleanupWorker : AsyncPeriodicBackgroundWorkerBase, ITransientDependency
{
    public EmailLogCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        // Run once a day
        Timer.Period = 24 * 60 * 60 * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var logRepository = workerContext
            .ServiceProvider
            .GetRequiredService<IRepository<EmailDeliveryLog, Guid>>();

        var cutOffDate = DateTime.Now.AddMonths(-6);

        // Deleting in batches ideal, but simple query for now.
        // EF Core 7+ supports execute delete, but keeping it repository-agnostic for standard ABP.
        // Assuming GetListAsync filtering or direct IQueryable access.
        
        await logRepository.DeleteAsync(x => x.Timestamp < cutOffDate);
    }
}
