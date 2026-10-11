using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantSubdomainRulesTests
{
    [Theory]
    [InlineData("arian")]
    [InlineData("arian-group")]
    [InlineData("shop2026")]
    [InlineData("a1b")]
    public void TryValidate_Should_Accept_A_Latin_Dns_Label(string value)
    {
        TenantSubdomainRules.TryValidate(value, out var normalized, out var reason)
            .ShouldBeTrue(reason);

        normalized.ShouldBe(value);
    }

    [Theory]
    [InlineData("arian گروه", "Characters")]
    [InlineData("arian group", "Characters")]
    [InlineData("arian_group", "Characters")]
    [InlineData("-arian", "EdgeHyphen")]
    [InlineData("arian-", "EdgeHyphen")]
    [InlineData("ab", "Length")]
    [InlineData("xn--arian", "Punycode")]
    [InlineData("", "Required")]
    public void TryValidate_Should_Reject_An_Illegal_Label(string value, string expectedReason)
    {
        TenantSubdomainRules.TryValidate(value, out _, out var reason)
            .ShouldBeFalse();

        reason.ShouldBe(expectedReason);
    }

    [Fact]
    public void TryValidate_Should_Normalize_Uppercase_Input_To_Lowercase()
    {
        TenantSubdomainRules.TryValidate("Arian", out var normalized, out _)
            .ShouldBeTrue();

        normalized.ShouldBe("arian");
    }

    [Fact]
    public void TryValidate_Should_Reject_A_Reserved_Subdomain()
    {
        TenantSubdomainRules.TryValidate("console", out _, out var reason)
            .ShouldBeFalse();

        reason.ShouldBe("Reserved");
    }
}
