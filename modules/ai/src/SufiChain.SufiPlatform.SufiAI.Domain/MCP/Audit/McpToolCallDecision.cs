namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// sufi-chain#384 slice 6a: what the pipeline decided about a tool call before (or instead of) running it.
/// A refused call is audited exactly like an allowed one, so an admin can see both.
/// </summary>
public enum McpToolCallDecision
{
    /// <summary>The call passed every check (binding, scope, permission, decision gates) and was dispatched.</summary>
    Allowed = 0,

    /// <summary>A check refused the call, so the tool never ran. <c>DecisionReason</c> carries the code.</summary>
    Denied = 1,

    /// <summary>A decision gate held the call for approval. The tool did not run in this turn.</summary>
    NeedsApproval = 2
}

/// <summary>
/// sufi-chain#384 slice 6a: how a dispatched tool call ended.
/// </summary>
public enum McpToolCallResultStatus
{
    /// <summary>The tool returned a successful result.</summary>
    Succeeded = 0,

    /// <summary>The tool ran and reported a failure (business error, validation error, transport error).</summary>
    Failed = 1,

    /// <summary>The turn's cancellation token stopped the call before it produced a result.</summary>
    Cancelled = 2
}
