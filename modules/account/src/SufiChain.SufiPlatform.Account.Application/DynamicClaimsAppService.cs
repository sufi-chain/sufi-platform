using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Account;

[Authorize]
public class DynamicClaimsAppService : SufiApplicationService, IDynamicClaimsAppService
{
    protected IdentityDynamicClaimsPrincipalContributorCache IdentityDynamicClaimsPrincipalContributorCache { get; }
    protected IAbpClaimsPrincipalFactory AbpClaimsPrincipalFactory { get; }
    protected ICurrentPrincipalAccessor PrincipalAccessor { get; }

    public DynamicClaimsAppService(
        IdentityDynamicClaimsPrincipalContributorCache identityDynamicClaimsPrincipalContributorCache,
        IAbpClaimsPrincipalFactory abpClaimsPrincipalFactory,
        ICurrentPrincipalAccessor principalAccessor)
    {
        IdentityDynamicClaimsPrincipalContributorCache = identityDynamicClaimsPrincipalContributorCache;
        AbpClaimsPrincipalFactory = abpClaimsPrincipalFactory;
        PrincipalAccessor = principalAccessor;
    }

    public virtual async Task RefreshAsync()
    {
        await IdentityDynamicClaimsPrincipalContributorCache.ClearAsync(
            CurrentUser.GetId(),
            CurrentUser.TenantId);

        await AbpClaimsPrincipalFactory.CreateDynamicAsync(PrincipalAccessor.Principal);
    }
}
