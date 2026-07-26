namespace SufiChain.SufiPlatform.Editions;

public static class EditionsDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiEditions.";
    public static string? DbSchema { get; set; } = null;
    public const string ConnectionStringName = "SufiEditions";
}
