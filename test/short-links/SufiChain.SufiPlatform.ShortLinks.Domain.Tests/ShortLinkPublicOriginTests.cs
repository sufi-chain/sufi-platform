using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortLinkPublicOriginTests
{
    [Fact]
    public void ShortLinkPublicUrlResolver_GetOrigin_MatchesGetAsyncOrigin()
    {
        var origin = ShortLinkPublicUrl.ResolveOrigin(
            "https://{0}.example",
            null,
            null,
            "tenant.example",
            "ignored");

        var full = ShortLinkPublicUrl.Build(origin, "abc");

        origin.ShouldBe("https://tenant.example");
        full.ShouldBe("https://tenant.example/s/abc");
        full.ShouldStartWith(origin!);
    }
}
