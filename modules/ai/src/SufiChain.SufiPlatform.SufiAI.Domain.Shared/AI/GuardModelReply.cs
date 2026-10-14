using System;
using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Detects a content-safety classifier verdict that was returned as the chat completion.
/// </summary>
public static class GuardModelReply
{
    // Anchored, bounded tokens, no nested .* — avoids catastrophic backtracking on ordinary replies.
    private const string VerdictPattern =
        @"^\s*User Safety:\s*[A-Za-z0-9_]{1,64}(?:\s+Response Safety:\s*[A-Za-z0-9_]{1,64})?\s*$";

    private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(1);

#if NET7_0_OR_GREATER
    private static readonly Regex Verdict = new(
        VerdictPattern,
        RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.NonBacktracking,
        MatchTimeout);
#else
    private static readonly Regex Verdict = new(
        VerdictPattern,
        RegexOptions.CultureInvariant | RegexOptions.Compiled,
        MatchTimeout);
#endif

    public static bool IsVerdict(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return false;
        }

        try
        {
            return Verdict.IsMatch(content);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
