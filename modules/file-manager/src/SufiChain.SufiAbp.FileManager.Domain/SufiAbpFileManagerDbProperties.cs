namespace SufiChain.SufiAbp.FileManager;

public static class SufiAbpFileManagerDbProperties
{
    public static string DbTablePrefix { get; set; } = "FileManager.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "FileManager";
}
