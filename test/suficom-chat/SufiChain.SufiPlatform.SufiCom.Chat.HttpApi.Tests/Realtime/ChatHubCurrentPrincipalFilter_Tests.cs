using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Realtime;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Realtime;

public class ChatHubCurrentPrincipalFilter_Tests
{
    [Fact]
    public void Request_tenant_is_kept_when_the_principal_has_no_tenant_claim()
    {
        var requestTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        ChatHubCurrentPrincipalFilter.ResolveTenantId(null, requestTenantId).ShouldBe(requestTenantId);
        ChatHubCurrentPrincipalFilter.ResolveTenantId(null, null).ShouldBeNull();
    }

    [Fact]
    public void Tenant_claim_wins_when_it_is_present()
    {
        var claimTenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var requestTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        ChatHubCurrentPrincipalFilter.ResolveTenantId(claimTenantId, requestTenantId).ShouldBe(claimTenantId);
    }
}
