using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp.Authorization.Permissions;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Execution;

/// <summary>
/// sufi-chain#384 slice 3: decides whether a tool is offered to the model for this turn's bound
/// caller. A tool the caller can't run is never shown to the model; execution still re-checks.
/// </summary>
public static class MCPToolOfferFilter
{
    public static async Task<bool> IsOfferedAsync(
        IMCPTool tool,
        AgentCallContext? call,
        IPermissionChecker? permissionChecker)
    {
        if (AnonymousRefusal(tool, call) is not null || ScopeRefusal(tool, call) is not null)
        {
            return false;
        }

        if (tool is not IMCPToolAccessRequirement requirement)
        {
            return true;
        }
        var needsUser = requirement.RequiresAuthentication || requirement.RequiresPermission;
        if (!needsUser)
        {
            return true;
        }

        if (call is null || call.IsAnonymous)
        {
            return false;
        }

        if (!requirement.RequiresPermission)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(requirement.PermissionName) || permissionChecker is null)
        {
            return false;
        }

        return await permissionChecker.IsGrantedAsync(call.Principal, requirement.PermissionName);
    }

    /// <summary>
    /// sufi-chain#384 slice 4: an anonymous (or unbound) caller may only use allowlisted tools.
    /// Returns the refusal code, or null when the call may proceed. External tools carry no allowlist yet.
    /// </summary>
    /// <summary>
    /// sufi-chain#384 slice 5: a read-only agent may only use tools explicitly marked read-only. Unmarked and
    /// external tools count as write (interim rule pending #138). Returns the refusal code, or null.
    /// </summary>
    public static string? ScopeRefusal(IMCPTool tool, AgentCallContext? call)
    {
        if (call?.AgentScope != AgentToolScope.ReadOnly)
        {
            return null;
        }

        return tool is IMCPToolEffect { IsReadOnly: true }
            ? null
            : AgentCallErrorCodes.AgentScopeReadOnly;
    }

    public static string? AnonymousRefusal(IMCPTool tool, AgentCallContext? call)
    {
        if (call is not null && !call.IsAnonymous)
        {
            return null;
        }

        return tool is IMCPToolAccessRequirement { AllowsAnonymous: true }
            ? null
            : AgentCallErrorCodes.AnonymousNotAllowed;
    }
}
