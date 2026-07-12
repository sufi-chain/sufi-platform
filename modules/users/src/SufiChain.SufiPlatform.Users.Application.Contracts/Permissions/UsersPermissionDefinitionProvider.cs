using SufiChain.SufiPlatform.Users.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Users.Permissions;

public class UsersPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var usersGroup = context.GetGroupOrNull(UsersPermissions.GroupName) ??
                         context.AddGroup(UsersPermissions.GroupName, L("Permission:Users"));

        usersGroup
            .AddPermission(UsersPermissions.UserLookup.Default, L("Permission:UserLookup"))
            .WithProviders(
                ClientPermissionValueProvider.ProviderName,
                SufiChain.SufiPlatform.Authorization.Permissions.RolePermissionValueProvider.ProviderName);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiUsersResource>(name);
    }
}
