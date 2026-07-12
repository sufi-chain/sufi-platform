namespace SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;

public static class SufiAIDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiAI.";
    public static string? DbSchema { get; set; } = null;
    public const string ConnectionStringName = "SufiAI";
}
