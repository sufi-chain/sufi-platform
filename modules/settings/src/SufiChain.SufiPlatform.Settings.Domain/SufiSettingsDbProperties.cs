using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.Settings;

public static class SufiSettingsDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiSettings.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiSettings";
}
