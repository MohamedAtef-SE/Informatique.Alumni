using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.TenantManagement;
using Informatique.Alumni.Profiles;
using Informatique.Alumni.Currencies;
using Volo.Abp.BackgroundWorkers;
using System.Threading.Tasks;
using Volo.Abp;

namespace Informatique.Alumni;

[DependsOn(
    typeof(AlumniDomainModule),
    typeof(AlumniApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class AlumniApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<AlumniApplicationMappers>();
        context.Services.AddTransient<IStudentSystemIntegrationService, SqlStudentSystemIntegrationService>();
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<ExchangeRateSyncWorker>();
    }
}
