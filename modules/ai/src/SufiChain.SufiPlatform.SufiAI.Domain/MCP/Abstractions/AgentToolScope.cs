namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// sufi-chain#384 slice 5: how far an agent may act for its user. Interim two-level rule (pending #138
/// access levels). An agent without a level (null on <see cref="AgentCallContext.AgentScope"/>) acts with the
/// user's own permissions. A level only ever narrows the user's permissions; it never adds to them.
/// </summary>
public enum AgentToolScope
{
    /// <summary>Only tools explicitly marked read-only. Unmarked tools count as write (fail closed).</summary>
    ReadOnly = 1,

    /// <summary>Every tool the bound user may run.</summary>
    Full = 2
}
