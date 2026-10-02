using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Detects a content-safety classifier verdict that was returned as the chat completion.
/// </summary>
public static class GuardModelReply
{
    private static readonly Regex Verdict = new(
        @"^\s*User Safety:\s*\w+(\s*Response Safety:\s*\w+)?\s*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(50));

    public static bool IsVerdict(string? content)
    {
        return !string.IsNullOrEmpty(content) && Verdict.IsMatch(content);
    }
}
