using Volo.Abp.Modularity;

namespace Informatique.Alumni;

[DependsOn(
    typeof(AlumniApplicationModule),
    typeof(AlumniDomainTestModule)
)]
public class AlumniApplicationTestModule : AbpModule
{

}
