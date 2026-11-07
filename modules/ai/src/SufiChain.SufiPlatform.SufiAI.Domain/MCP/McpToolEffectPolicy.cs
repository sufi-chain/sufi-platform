using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.MCP;

/// <summary>
/// sufi-chain#557: an external tool is Read only when an admin stored Read or Write for that tool.
/// The catalog <c>readOnlyHint</c> is not an input. Omitting it here is the policy.
/// </summary>
public static class McpToolEffectPolicy
{
    public static AgentActionEffect Resolve(AgentActionEffect? adminEffect)
    {
        return adminEffect is AgentActionEffect.Read or AgentActionEffect.Write
            ? adminEffect.Value
            : AgentActionEffect.Write;
    }
}
