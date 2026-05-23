namespace SufiChain.SufiAbp.LocalizationManagement;

public static class SufiAbpLocalizationManagementDbProperties
{
    public static string DbTablePrefix { get; set; } = "LocalizationManagement.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "LocalizationManagement";
}
