using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortLinkPublicUrlTests
{
    [Fact]
    public void Dedicated_domain_keeps_the_public_scheme_and_port()
    {
        var origin = ShortLinkPublicUrl.ResolveOrigin(
            "https://sufichain.com:8443",
            "https://console.sufichain.com",
            null,
            "shop.example",
            "demo");

        origin.ShouldBe("https://shop.example:8443");
        ShortLinkPublicUrl.Build(origin, "abc").ShouldBe("https://shop.example:8443/s/abc");
    }

    [Fact]
    public void Generated_host_is_the_public_origin_when_there_is_no_custom_domain()
    {
        var origin = ShortLinkPublicUrl.ResolveOrigin(
            "https://public.example",
            null,
            null,
            "demo.sufichain.com",
            "demo");

        ShortLinkPublicUrl.Build(origin, "abc").ShouldBe("https://demo.sufichain.com/s/abc");
    }

    [Fact]
    public void Subdomain_placeholder_is_used_when_no_host_is_stored()
    {
        var origin = ShortLinkPublicUrl.ResolveOrigin(
            "https://{0}.sufichain.com",
            null,
            null,
            null,
            "demo");

        ShortLinkPublicUrl.Build(origin, "abc").ShouldBe("https://demo.sufichain.com/s/abc");
    }

    [Fact]
    public void Host_links_use_the_public_root_and_not_a_placeholder()
    {
        var origin = ShortLinkPublicUrl.ResolveOrigin(
            "https://{0}.sufichain.com",
            "https://console.sufichain.com",
            null,
            null,
            null);

        ShortLinkPublicUrl.Build(origin, "abc").ShouldBe("https://sufichain.com/s/abc");
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("shop.example/extra")]
    [InlineData("not a host")]
    public void Unsafe_hosts_are_not_used(string host)
    {
        ShortLinkPublicUrl.ResolveOrigin("https://sufichain.com", null, null, host, "demo")
            .ShouldBeNull();
    }
}
