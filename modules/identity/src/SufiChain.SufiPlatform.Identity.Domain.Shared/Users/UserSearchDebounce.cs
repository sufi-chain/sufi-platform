namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// When a users-list search box should hit the server.
/// Typing waits. Enter and a cleared box apply at once.
/// </summary>
public static class UserSearchDebounce
{
    public const int DelayMs = 300;

    public static bool ApplyImmediately(string? key, string? value) =>
        string.Equals(key, "Enter", StringComparison.Ordinal) || string.IsNullOrEmpty(value);
}
