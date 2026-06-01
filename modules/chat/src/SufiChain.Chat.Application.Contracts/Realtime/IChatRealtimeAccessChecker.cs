namespace SufiChain.Chat.Realtime;

public interface IChatRealtimeAccessChecker
{
    Task<bool> CanJoinSessionAsync(Guid sessionId, string? anonymousVisitorId = null);
}
