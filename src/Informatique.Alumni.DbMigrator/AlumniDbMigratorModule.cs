using Informatique.Alumni.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Informatique.Alumni.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AlumniEntityFrameworkCoreModule),
    typeof(AlumniApplicationContractsModule)
)]
public class AlumniDbMigratorModule : AbpModule
{
}
