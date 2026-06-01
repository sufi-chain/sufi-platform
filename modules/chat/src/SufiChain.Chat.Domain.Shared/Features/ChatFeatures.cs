namespace SufiChain.Chat.Features;

public static class ChatFeatures
{
    public const string GroupName = "Chat";
    public const string Enable = GroupName + ".Enable";
    public const string PublicWidget = GroupName + ".PublicWidget";
    public const string Attachments = GroupName + ".Attachments";
    public const string Realtime = GroupName + ".Realtime";
    public const string EmailConnector = GroupName + ".EmailConnector";

    public static class Ai
    {
        public const string Enable = GroupName + ".Ai.Enable";
        public const string UsageGuard = GroupName + ".Ai.UsageGuard";
        public const string AnonymousHandoff = GroupName + ".Ai.AnonymousHandoff";
    }
}
