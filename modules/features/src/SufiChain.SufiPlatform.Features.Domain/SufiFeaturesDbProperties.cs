using SufiChain.SufiPlatform.Data;

namespace SufiChain.SufiPlatform.Features;

public static class SufiFeaturesDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiFeatures.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiFeatures";
}
