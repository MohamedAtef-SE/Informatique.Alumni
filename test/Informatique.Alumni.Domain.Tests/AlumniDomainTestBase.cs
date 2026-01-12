using Volo.Abp.Modularity;

namespace Informatique.Alumni;

/* Inherit from this class for your domain layer tests. */
public abstract class AlumniDomainTestBase<TStartupModule> : AlumniTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
