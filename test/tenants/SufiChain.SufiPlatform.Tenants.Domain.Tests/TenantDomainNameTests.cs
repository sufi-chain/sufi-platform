using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantDomainNameTests
{
    [Fact]
    public void NormalizeHost_Should_Return_A_Dns_Host()
    {
        TenantDomainName.NormalizeHost("Shop.Example.com").ShouldBe("shop.example.com");
    }

    [Fact]
    public void NormalizeHost_Should_Reject_An_Empty_Or_Invalid_Host()
    {
        Should.Throw<ArgumentException>(() => TenantDomainName.NormalizeHost(" "));
        Should.Throw<ArgumentException>(() => TenantDomainName.NormalizeHost("https://example.com"));
    }
}
