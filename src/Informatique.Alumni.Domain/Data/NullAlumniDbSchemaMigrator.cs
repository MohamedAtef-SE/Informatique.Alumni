using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni.Data;

/* This is used if database provider does't define
 * IAlumniDbSchemaMigrator implementation.
 */
public class NullAlumniDbSchemaMigrator : IAlumniDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
