using Informatique.Alumni.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Informatique.Alumni.Permissions;

public class AlumniPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(AlumniPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(AlumniPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AlumniResource>(name);
    }
}
