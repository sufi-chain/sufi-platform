using SufiChain.SufiPlatform.AuditLogging.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.AuditLogging.Permissions;

/// <summary>
/// Defines permissions for the Audit Logging module.
/// </summary>
public class AuditLoggingPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var auditLoggingGroup = context.AddGroup(
            AuditLoggingPermissions.GroupName,
            L("Permission:AuditLogging"));

        auditLoggingGroup.AddPermission(
            AuditLoggingPermissions.AuditLogs.Default,
            L("Permission:AuditLogs"));

        auditLoggingGroup.AddPermission(
            AuditLoggingPermissions.EntityChanges.Default,
            L("Permission:EntityChanges"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAuditLoggingResource>(name);
    }
}
