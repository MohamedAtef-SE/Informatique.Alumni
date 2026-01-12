using Microsoft.Extensions.Localization;
using Informatique.Alumni.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Informatique.Alumni;

[Dependency(ReplaceServices = true)]
public class AlumniBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<AlumniResource> _localizer;

    public AlumniBrandingProvider(IStringLocalizer<AlumniResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
