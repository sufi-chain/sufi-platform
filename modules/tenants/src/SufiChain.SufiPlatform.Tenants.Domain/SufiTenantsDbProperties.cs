using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.Tenants;

public static class SufiTenantsDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiTenants.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiTenants";
}
