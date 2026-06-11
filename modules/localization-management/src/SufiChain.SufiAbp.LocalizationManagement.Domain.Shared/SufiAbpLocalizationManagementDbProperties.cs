namespace SufiChain.SufiAbp.LocalizationManagement;

public static class SufiAbpLocalizationManagementDbProperties
{
    public static string DbTablePrefix { get; set; } = "Localization.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "LocalizationManagement";
}
