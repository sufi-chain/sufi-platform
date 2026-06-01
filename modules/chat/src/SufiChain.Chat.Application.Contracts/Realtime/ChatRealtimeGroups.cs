namespace SufiChain.Chat.Realtime;

public static class ChatRealtimeGroups
{
    public static string Session(Guid sessionId)
    {
        return $"chat-session-{sessionId:N}";
    }

    public static string User(Guid userId)
    {
        return $"chat-user-{userId:N}";
    }
}
