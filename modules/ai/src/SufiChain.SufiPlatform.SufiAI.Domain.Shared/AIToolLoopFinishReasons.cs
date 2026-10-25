using System;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// sufi-chain#340: finish reasons set on the reply when the tool executor stopped a turn, so callers record the
/// turn as stopped instead of successful.
/// </summary>
public static class AIToolLoopFinishReasons
{
    /// <summary>
    /// Legacy: the turn hit the old <c>MaxTurnSeconds</c> wall-clock cutoff. 9.4 removed that cutoff (a turn is
    /// cancelled only by the user, session close, or shutdown), so nothing sets this any more. The constant and
    /// its status mapping stay so already-persisted rows and sibling repos keep compiling.
    /// </summary>
    public const string TimeLimit = "time_limit";

    /// <summary>The turn hit <c>AIToolLoopOptions.MaxToolRounds</c>.</summary>
    public const string ToolRoundLimit = "tool_round_limit";

    public static bool IsLimit(string? finishReason) =>
        string.Equals(finishReason, TimeLimit, StringComparison.Ordinal)
        || string.Equals(finishReason, ToolRoundLimit, StringComparison.Ordinal);
}
