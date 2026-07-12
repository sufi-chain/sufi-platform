using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.Permissions;

public static class SufiPermissionsDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiPermissions.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiPermissions";
}
