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
}
