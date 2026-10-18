namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// What a tool needs from the caller before it may be offered or run (sufi-chain#384).
/// Tools that don't implement this (external MCP tools, until slice 7) declare no requirement.
/// </summary>
public interface IMCPToolAccessRequirement
{
    /// <summary>Permission the bound user must hold, or null.</summary>
    string? PermissionName { get; }

    /// <summary>True when the tool declares a permission requirement (fails closed without a name).</summary>
    bool RequiresPermission { get; }

    /// <summary>True when the tool needs a signed-in user (e.g. <c>[Authorize]</c> without a policy).</summary>
    bool RequiresAuthentication { get; }

    /// <summary>
    /// True when anonymous turns may be offered and run this tool: <c>Public</c> + <c>ReadOnly</c>, or an
    /// explicit <c>[SufiAiMcpPublicWrite]</c> allowlist entry (sufi-chain#384 slice 4).
    /// </summary>
    bool AllowsAnonymous { get; }
}
