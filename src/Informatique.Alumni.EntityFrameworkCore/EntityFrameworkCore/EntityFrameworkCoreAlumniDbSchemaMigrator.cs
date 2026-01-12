using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Informatique.Alumni.Data;
using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni.EntityFrameworkCore;

public class EntityFrameworkCoreAlumniDbSchemaMigrator
    : IAlumniDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreAlumniDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the AlumniDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<AlumniDbContext>()
            .Database
            .MigrateAsync();
    }
}
