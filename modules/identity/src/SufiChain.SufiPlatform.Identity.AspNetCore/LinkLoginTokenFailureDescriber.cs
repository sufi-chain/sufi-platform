using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Identity.AspNetCore;

public class LinkLoginTokenFailureDescriber : ILinkLoginTokenFailureDescriber, ITransientDependency
{
    protected LinkUserTokenProvider TokenProvider { get; }
    protected IdentityUserManager UserManager { get; }
    protected ICurrentTenant CurrentTenant { get; }

    public LinkLoginTokenFailureDescriber(
        LinkUserTokenProvider tokenProvider,
        IdentityUserManager userManager,
        ICurrentTenant currentTenant)
    {
        TokenProvider = tokenProvider;
        UserManager = userManager;
        CurrentTenant = currentTenant;
    }

    public virtual async Task<string> DescribeAsync(
        IdentityLinkUserInfo source,
        string token,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        using (CurrentTenant.Change(source.TenantId))
        {
            var user = await UserManager.FindByIdAsync(source.UserId.ToString());
            if (user == null)
            {
                return LinkLoginRejectionReasons.SourceUserNotFound;
            }

            var userId = await UserManager.GetUserIdAsync(user);
            var supportsStamp = UserManager.SupportsUserSecurityStamp;
            var stamp = supportsStamp
                ? await UserManager.GetSecurityStampAsync(user)
                : null;
            return TokenProvider.DescribeFailure(token, purpose, userId, stamp, supportsStamp);
        }
    }
}