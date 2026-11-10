using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Who may open AI usage analytics. The admin AI permission
/// (<see cref="AIPermissions.AI.Default"/>) is required. A chat grant is not enough.
/// The page also lists workspaces, so that read still accepts the workspace permission or this admin grant.
/// </summary>
public static class AIUsageAccess
{
    public static async Task CheckCanViewUsageAsync(IPermissionChecker permissionChecker)
    {
        if (!await permissionChecker.IsGrantedAsync(AIPermissions.AI.Default))
        {
            throw new AbpAuthorizationException(code: AbpAuthorizationErrorCodes.GivenPolicyHasNotGrantedWithPolicyName)
                .WithData("PolicyName", AIPermissions.AI.Default);
        }
    }

    public static async Task CheckCanListWorkspacesAsync(IPermissionChecker permissionChecker)
    {
        if (await permissionChecker.IsGrantedAsync(AIPermissions.Workspaces.Default)
            || await permissionChecker.IsGrantedAsync(AIPermissions.AI.Default)
            || await permissionChecker.IsGrantedAsync(AIPermissions.WorkspaceChat.Default)
            || await permissionChecker.IsGrantedAsync(AIPermissions.AI.Chat))
        {
            return;
        }

        throw new AbpAuthorizationException(code: AbpAuthorizationErrorCodes.GivenPolicyHasNotGrantedWithPolicyName)
            .WithData("PolicyName", AIPermissions.Workspaces.Default);
    }
}
