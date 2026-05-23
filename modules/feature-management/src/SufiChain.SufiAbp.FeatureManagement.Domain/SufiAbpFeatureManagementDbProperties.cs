using SufiChain.SufiAbp.Data;

namespace SufiChain.SufiAbp.FeatureManagement;

public static class SufiAbpFeatureManagementDbProperties
{
    public static string DbTablePrefix { get; set; } = "Features.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiAbpFeatureManagement";
}
