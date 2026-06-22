namespace SufiChain.SufiAbp.AI.EntityFrameworkCore;

public static class AIDbProperties
{
    public static string DbTablePrefix { get; set; } = "AI.";
    public static string? DbSchema { get; set; } = null;
    public const string ConnectionStringName = "AI";
}
