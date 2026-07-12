namespace SufiChain.SufiPlatform.FileManager;

public static class SufiFileManagerDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiFileManager.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiFileManager";
}