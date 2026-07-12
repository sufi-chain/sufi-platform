using System;
using System.Collections.Generic;
using System.Text;
using SufiChain.SufiPlatform.Tenants.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tenants;
public class SufiTenantsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var tenantManagementGroup = context.AddGroup(TenantsPermissions.GroupName, L("Permission:Tenants"));

        var tenantsPermission = tenantManagementGroup.AddPermission(TenantsPermissions.Tenants.Default, L("Permission:Tenants"), multiTenancySide: MultiTenancySides.Host);
        tenantsPermission.AddChild(TenantsPermissions.Tenants.Create, L("Permission:Create"), multiTenancySide: MultiTenancySides.Host);
        tenantsPermission.AddChild(TenantsPermissions.Tenants.Update, L("Permission:Edit"), multiTenancySide: MultiTenancySides.Host);
        tenantsPermission.AddChild(TenantsPermissions.Tenants.Delete, L("Permission:Delete"), multiTenancySide: MultiTenancySides.Host);
        tenantsPermission.AddChild(TenantsPermissions.Tenants.ManageFeatures, L("Permission:ManageFeatures"), multiTenancySide: MultiTenancySides.Host);
        tenantsPermission.AddChild(TenantsPermissions.Tenants.ManageConnectionStrings, L("Permission:ManageConnectionStrings"), multiTenancySide: MultiTenancySides.Host);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiTenantsResource>(name);
    }
}
