namespace SufiChain.SufiAbp.AIManagement.EntityFrameworkCore;

public static class AIManagementDbProperties
{
    public static string DbTablePrefix { get; set; } = "AIManagement.";
    public static string? DbSchema { get; set; } = null;
    public const string ConnectionStringName = "AIManagement";
}
