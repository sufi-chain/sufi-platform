using Volo.Abp.Data;

namespace SufiChain.SufiAbp.PermissionManagement;

public static class SufiAbpPermissionManagementDbProperties
{
    public static string DbTablePrefix { get; set; } = "Permissions.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiAbpPermissionManagement";
}
