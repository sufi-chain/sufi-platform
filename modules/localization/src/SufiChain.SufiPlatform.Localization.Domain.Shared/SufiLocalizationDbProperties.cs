namespace SufiChain.SufiPlatform.Localization;

public static class SufiLocalizationDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiLocalization.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiLocalization";
}
