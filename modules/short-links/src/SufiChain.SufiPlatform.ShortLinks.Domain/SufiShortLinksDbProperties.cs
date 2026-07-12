namespace SufiChain.SufiPlatform.ShortLinks;

public static class SufiShortLinksDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiShortLinks.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiShortLinks";
}