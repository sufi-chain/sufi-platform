using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// sufi-chain#411: who may open AI usage analytics. «مشاهده مصرف» (<see cref="AIPermissions.AI.ViewUsage"/>)
/// alone is enough. The page also lists the workspaces and their guardrail status, so those two reads accept
/// either the workspace permission or ViewUsage.
/// </summary>
public static class AIUsageAccess
{
    public static async Task CheckCanViewUsageAsync(IPermissionChecker permissionChecker)
    {
        if (!await permissionChecker.IsGrantedAsync(AIPermissions.AI.ViewUsage))
        {
            throw new AbpAuthorizationException(code: AbpAuthorizationErrorCodes.GivenPolicyHasNotGrantedWithPolicyName)
                .WithData("PolicyName", AIPermissions.AI.ViewUsage);
        }
    }

    public static async Task CheckCanListWorkspacesAsync(IPermissionChecker permissionChecker)
    {
        if (await permissionChecker.IsGrantedAsync(AIPermissions.Workspaces.Default)
            || await permissionChecker.IsGrantedAsync(AIPermissions.AI.ViewUsage))
        {
            return;
        }

        throw new AbpAuthorizationException(code: AbpAuthorizationErrorCodes.GivenPolicyHasNotGrantedWithPolicyName)
            .WithData("PolicyName", AIPermissions.Workspaces.Default);
    }
}
