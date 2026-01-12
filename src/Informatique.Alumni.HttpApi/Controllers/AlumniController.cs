using Informatique.Alumni.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Informatique.Alumni.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class AlumniController : AbpControllerBase
{
    protected AlumniController()
    {
        LocalizationResource = typeof(AlumniResource);
    }
}
