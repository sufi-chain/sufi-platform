using MyCompanyName.MyProjectName.Localization;
using SufiChain.SufiPlatform.Authorization.Permissions;

namespace MyCompanyName.MyProjectName.Permissions
{
    public class DemoAppPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var myGroup = context.AddGroup(DemoAppPermissions.GroupName);
            //Define your own permissions here. Example:
            //myGroup.AddPermission(DemoAppPermissions.MyPermission1, L("Permission:MyPermission1"));
        }
    }
}
