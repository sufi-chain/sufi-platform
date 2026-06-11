namespace SufiChain.SufiAbp.ShortLinkGenerator;

public static class SufiAbpShortLinkGeneratorDbProperties
{
    public static string DbTablePrefix { get; set; } = "ShortLinks.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "ShortLinkGenerator";
}
