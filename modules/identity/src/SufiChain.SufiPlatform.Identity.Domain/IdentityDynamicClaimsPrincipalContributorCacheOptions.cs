using System;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityDynamicClaimsPrincipalContributorCacheOptions
{
    public TimeSpan CacheAbsoluteExpiration { get; set; }

    public IdentityDynamicClaimsPrincipalContributorCacheOptions()
    {
        CacheAbsoluteExpiration = TimeSpan.FromHours(1);
    }
}
