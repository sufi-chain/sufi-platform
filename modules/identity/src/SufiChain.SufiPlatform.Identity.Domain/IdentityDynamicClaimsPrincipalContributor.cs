using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Security.Claims;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityDynamicClaimsPrincipalContributor : AbpDynamicClaimsPrincipalContributorBase
{
    public async override Task ContributeAsync(AbpClaimsPrincipalContributorContext context)
    {
        var identity = context.ClaimsPrincipal.Identities.FirstOrDefault();
        var userId = identity?.FindUserId();
        if (userId == null)
        {
            return;
        }

        var dynamicClaimsCache = context.GetRequiredService<IdentityDynamicClaimsPrincipalContributorCache>();
        AbpDynamicClaimCacheItem dynamicClaims;
        try
        {
            dynamicClaims = await dynamicClaimsCache.GetAsync(userId.Value, identity!.FindTenantId());
        }
        catch (EntityNotFoundException e)
        {
            context.ClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            var logger = context.GetRequiredService<ILogger<IdentityDynamicClaimsPrincipalContributor>>();
            logger.LogWarning(e, "User not found: {UserId}", userId.Value);
            return;
        }

        if (dynamicClaims.Claims.IsNullOrEmpty())
        {
            return;
        }

        await AddDynamicClaimsAsync(context, identity, dynamicClaims.Claims);
    }
}
