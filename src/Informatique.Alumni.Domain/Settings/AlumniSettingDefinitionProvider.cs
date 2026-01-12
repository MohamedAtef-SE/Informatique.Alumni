using Volo.Abp.Settings;

namespace Informatique.Alumni.Settings;

public class AlumniSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(AlumniSettings.MySetting1));
    }
}
