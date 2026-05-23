using Volo.Abp.Reflection;

namespace SufiChain.SufiAbp.AuditLogging.Permissions;

/// <summary>
/// Permission constants for the Audit Logging module.
/// Follows ABP permission naming conventions.
/// </summary>
public static class AuditLoggingPermissions
{
    public const string GroupName = "AuditLogging";

    public static class AuditLogs
    {
        public const string Default = GroupName + ".AuditLogs";
    }

    public static class EntityChanges
    {
        public const string Default = GroupName + ".EntityChanges";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(AuditLoggingPermissions));
    }
}
