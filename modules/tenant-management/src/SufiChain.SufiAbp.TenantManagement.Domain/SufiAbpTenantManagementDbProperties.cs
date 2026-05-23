using Volo.Abp.Data;

namespace SufiChain.SufiAbp.TenantManagement;

public static class SufiAbpTenantManagementDbProperties
{
    public static string DbTablePrefix { get; set; } = "Tenants.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiAbpTenantManagement";
}
