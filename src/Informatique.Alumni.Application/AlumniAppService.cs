using Informatique.Alumni.Localization;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni;

/* Inherit your application services from this class.
 */
public abstract class AlumniAppService : ApplicationService
{
    protected AlumniAppService()
    {
        LocalizationResource = typeof(AlumniResource);
    }
}
