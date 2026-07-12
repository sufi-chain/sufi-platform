using Volo.Abp.Authorization.Permissions;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Localization;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Identity.Permissions;

/// <summary>
/// Defines permissions for the Sufi Identity module.
/// </summary>
public class IdentityPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var identityGroup = context.GetGroupOrNull(IdentityPermissions.GroupName) ??
                            context.AddGroup(IdentityPermissions.GroupName, L("Permission:IdentityManagement"));

        DefineRolePermissions(identityGroup);
        DefineUserPermissions(identityGroup);
        DefineUserLookupPermissions(identityGroup);
        DefineSecurityLogPermissions(identityGroup);
        DefineOrganizationUnitPermissions(identityGroup);
    }

    private static void DefineRolePermissions(PermissionGroupDefinition group)
    {
        var rolesPermission = group.AddPermission(
            IdentityPermissions.Roles.Default,
            L("Permission:RoleManagement"));

        rolesPermission.AddChild(IdentityPermissions.Roles.Create, L("Permission:Create"));
        rolesPermission.AddChild(IdentityPermissions.Roles.Update, L("Permission:Edit"));
        rolesPermission.AddChild(IdentityPermissions.Roles.Delete, L("Permission:Delete"));
        rolesPermission.AddChild(IdentityPermissions.Roles.ManagePermissions, L("Permission:ChangePermissions"));
    }

    private static void DefineUserPermissions(PermissionGroupDefinition group)
    {
        var usersPermission = group.AddPermission(
            IdentityPermissions.Users.Default,
            L("Permission:UserManagement"));

        usersPermission.AddChild(IdentityPermissions.Users.Create, L("Permission:Create"));
        var editPermission = usersPermission.AddChild(IdentityPermissions.Users.Update, L("Permission:Edit"));
        editPermission.AddChild(IdentityPermissions.Users.ManageRoles, L("Permission:ManageRoles"));
        usersPermission.AddChild(IdentityPermissions.Users.Delete, L("Permission:Delete"));
        usersPermission.AddChild(IdentityPermissions.Users.ManagePermissions, L("Permission:ChangePermissions"));
    }

    private static void DefineUserLookupPermissions(PermissionGroupDefinition group)
    {
        group
            .AddPermission(IdentityPermissions.UserLookup.Default, L("Permission:UserLookup"))
            .WithProviders(
                ClientPermissionValueProvider.ProviderName,
                SufiChain.SufiPlatform.Authorization.Permissions.RolePermissionValueProvider.ProviderName);
    }

    private static void DefineSecurityLogPermissions(PermissionGroupDefinition group)
    {
        group.AddPermission(
            IdentityPermissions.SecurityLogs.Default,
            L("Permission:SecurityLogs"));
    }

    private static void DefineOrganizationUnitPermissions(PermissionGroupDefinition group)
    {
        var organizationUnitsPermission = group.AddPermission(
            IdentityPermissions.OrganizationUnits.Default,
            L("Permission:OrganizationUnits"));

        organizationUnitsPermission.AddChild(
            IdentityPermissions.OrganizationUnits.Create,
            L("Permission:OrganizationUnits.Create"));

        organizationUnitsPermission.AddChild(
            IdentityPermissions.OrganizationUnits.Update,
            L("Permission:OrganizationUnits.Update"));

        organizationUnitsPermission.AddChild(
            IdentityPermissions.OrganizationUnits.Delete,
            L("Permission:OrganizationUnits.Delete"));

        organizationUnitsPermission.AddChild(
            IdentityPermissions.OrganizationUnits.ManageMembers,
            L("Permission:OrganizationUnits.ManageMembers"));

        organizationUnitsPermission.AddChild(
            IdentityPermissions.OrganizationUnits.ManageRoles,
            L("Permission:OrganizationUnits.ManageRoles"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiIdentityResource>(name);
    }
}
