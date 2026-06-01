namespace SufiChain.Chat;

/// <summary>
/// Well-known FileManager entity type names for Chat-owned files.
/// </summary>
public static class ChatEntityTypes
{
    /// <summary>
    /// Files scoped to a chat session via <see cref="ChatEntityTypes.Session"/> + session id.
    /// </summary>
    public const string Session = "Chat.Session";
}
