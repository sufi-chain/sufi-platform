namespace SufiChain.SufiPlatform.Calendar.AI;

/// <summary>
/// When the calendar assistant clears Thinking, and that a reply after the 45s hint is still shown.
/// </summary>
public static class CalendarAssistantThinking
{
    public static bool ClearsWhenStreamEnds(string? stage, string? status)
    {
        return string.Equals(stage, "writing_reply", StringComparison.OrdinalIgnoreCase)
            && string.Equals(status, "succeeded", StringComparison.OrdinalIgnoreCase);
    }

    public static bool ClearsWhenConfirmationShows(string? stage, string? status)
    {
        return string.Equals(stage, "tool_result", StringComparison.OrdinalIgnoreCase)
            && string.Equals(status, "succeeded", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The 45 second wait hint does not end the turn. A reply that arrives after the hint is still applied.
    /// </summary>
    public static bool ShowsLateReplyAfterHint() => true;
}
