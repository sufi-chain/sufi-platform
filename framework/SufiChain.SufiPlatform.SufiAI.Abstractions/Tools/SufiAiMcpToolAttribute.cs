using System;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Marks a service method as an AI/MCP tool that providers can discover
/// and expose to AI models.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class SufiAiMcpToolAttribute : Attribute
{
    /// <summary>
    /// Tool name (must be unique across all tools).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Human-readable description of what the tool does.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Optional category for grouping tools.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether this tool requires a permission check before execution.
    /// </summary>
    public bool RequiresPermission { get; set; }

    /// <summary>
    /// Permission name required to execute this tool.
    /// </summary>
    public string? PermissionName { get; set; }

    /// <summary>
    /// The tool only reads data. Assistant hooshvares can select it. Copilots can select every tool.
    /// Prefer <see cref="Effect"/>. <see cref="AgentActionEffect.Read"/> and <c>ReadOnly = true</c> agree;
    /// <c>ReadOnly = true</c> together with <see cref="AgentActionEffect.Write"/> is rejected at registration.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// What the tool does to data (sufi-chain#312). <see cref="AgentActionEffect.Unspecified"/> means not set:
    /// a read-only tool resolves to <see cref="AgentActionEffect.Read"/> and every other tool resolves to
    /// <see cref="AgentActionEffect.Write"/>.
    /// </summary>
    public AgentActionEffect Effect { get; set; }

    /// <summary>
    /// Resolves the effect the registrar stores. Fails when <see cref="ReadOnly"/> and an explicit write
    /// <see cref="Effect"/> disagree, or when <see cref="Effect"/> is not a defined value.
    /// </summary>
    public bool TryGetEffect(out AgentActionEffect effect, out string? refusal)
    {
        if (!Enum.IsDefined(typeof(AgentActionEffect), Effect))
        {
            effect = default;
            refusal = "The tool effect is not Read or Write.";
            return false;
        }

        if (ReadOnly && Effect == AgentActionEffect.Write)
        {
            effect = default;
            refusal = "A read-only tool cannot declare a write effect.";
            return false;
        }

        effect = Effect == AgentActionEffect.Unspecified
            ? (ReadOnly ? AgentActionEffect.Read : AgentActionEffect.Write)
            : Effect;
        refusal = null;
        return true;
    }

    /// <summary>
    /// The tool is safe for the public. Together with a read effect it may be offered to and run by
    /// anonymous turns (LiveChat visitors, public chat). Anonymous turns get nothing else unless the method is
    /// also marked <see cref="SufiAiMcpPublicWriteAttribute"/>.
    /// </summary>
    public bool Public { get; set; }
    /// <summary>
    /// Creates the attribute.
    /// </summary>
    public SufiAiMcpToolAttribute(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }
}
