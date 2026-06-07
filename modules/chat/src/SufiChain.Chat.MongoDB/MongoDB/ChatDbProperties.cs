namespace SufiChain.Chat.MongoDB;

public static class ChatDbProperties
{
    public static string DbTablePrefix { get; set; } = "Chat.";
    public const string ConnectionStringName = "Chat";
}
