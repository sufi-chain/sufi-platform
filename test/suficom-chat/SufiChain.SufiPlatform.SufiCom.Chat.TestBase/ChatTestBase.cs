using System.Security.Claims;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.Testing;

namespace SufiChain.SufiPlatform.SufiCom.Chat;

public abstract class ChatTestBase<TStartupModule> : AbpIntegratedTest<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected ICurrentTenant CurrentTenant => GetRequiredService<ICurrentTenant>();

    protected ChatTestCurrentUser CurrentUser => new(GetRequiredService<ICurrentPrincipalAccessor>());

    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    protected sealed class ChatTestCurrentUser
    {
        private readonly ICurrentPrincipalAccessor _principalAccessor;

        public ChatTestCurrentUser(ICurrentPrincipalAccessor principalAccessor)
        {
            _principalAccessor = principalAccessor;
        }

        public IDisposable Change(Guid? userId)
        {
            if (!userId.HasValue)
            {
                return _principalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var identity = new ClaimsIdentity("ChatTest");
            identity.AddClaim(new Claim(AbpClaimTypes.UserId, userId.Value.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            return _principalAccessor.Change(new ClaimsPrincipal(identity));
        }
    }
}
