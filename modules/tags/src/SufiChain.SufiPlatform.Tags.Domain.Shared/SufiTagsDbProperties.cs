namespace SufiChain.SufiPlatform.Tags;

public static class SufiTagsDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiTags.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiTags";
}