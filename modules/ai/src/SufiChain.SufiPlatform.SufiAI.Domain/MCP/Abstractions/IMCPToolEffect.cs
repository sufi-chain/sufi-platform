using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// sufi-chain#312: what a tool does to data. A tool that doesn't implement this counts as write.
/// <see cref="IsReadOnly"/> is true only for <see cref="AgentActionEffect.Read"/>.
/// </summary>
public interface IMCPToolEffect
{
    /// <summary>Read or write. Unmarked tools are <see cref="AgentActionEffect.Write"/>.</summary>
    AgentActionEffect Effect { get; }

    /// <summary>True only when <see cref="Effect"/> is <see cref="AgentActionEffect.Read"/>.</summary>
    bool IsReadOnly { get; }
}
