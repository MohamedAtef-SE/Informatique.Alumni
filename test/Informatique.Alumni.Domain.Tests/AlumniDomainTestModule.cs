using Volo.Abp.Modularity;

namespace Informatique.Alumni;

[DependsOn(
    typeof(AlumniDomainModule),
    typeof(AlumniTestBaseModule)
)]
public class AlumniDomainTestModule : AbpModule
{

}
