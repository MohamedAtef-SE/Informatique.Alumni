using Volo.Abp.Modularity;

namespace Informatique.Alumni;

public abstract class AlumniApplicationTestBase<TStartupModule> : AlumniTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
