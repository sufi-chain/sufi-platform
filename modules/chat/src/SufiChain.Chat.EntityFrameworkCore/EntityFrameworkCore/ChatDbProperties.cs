namespace SufiChain.Chat.EntityFrameworkCore;

public static class ChatDbProperties
{
    public static string DbTablePrefix { get; set; } = "Chat.";
    public static string? DbSchema { get; set; } = null;
    public const string ConnectionStringName = "Chat";
}
