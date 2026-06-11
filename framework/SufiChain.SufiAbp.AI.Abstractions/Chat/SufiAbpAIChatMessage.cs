namespace SufiChain.SufiAbp.AI;

/// <summary>
/// A single message in a chat completion conversation.
/// </summary>
public class SufiAbpAIChatMessage
{
    /// <summary>
    /// Message role. Use the constants on <see cref="SufiAbpAIChatRoles"/>.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Message text content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Well-known chat message roles.
/// </summary>
public static class SufiAbpAIChatRoles
{
    /// <summary>
    /// System/instruction role.
    /// </summary>
    public const string System = "system";

    /// <summary>
    /// End-user role.
    /// </summary>
    public const string User = "user";

    /// <summary>
    /// Model/assistant role.
    /// </summary>
    public const string Assistant = "assistant";
}
