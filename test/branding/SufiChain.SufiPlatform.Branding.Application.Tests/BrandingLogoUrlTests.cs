using Shouldly;
using SufiChain.SufiPlatform.Branding.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Branding.Branding;

public class BrandingLogoUrlTests
{
    private static readonly Guid MissingFileId = Guid.Parse("f0e042e6-243d-2b20-7d13-3a2419943a83");

    [Fact]
    public void Resolve_uses_the_default_logo_when_the_tenant_file_is_missing()
    {
        var configured = $"/api/file-manager/file-items/{MissingFileId}/download?__tenant=1eadd5c8243d2b207d133a2419943a83";
        var lookups = 0;

        var resolved = BrandingLogoUrls.Resolve(configured, id =>
        {
            lookups++;
            id.ShouldBe(MissingFileId);
            return false;
        });

        resolved.ShouldBe(BrandingLogoUrls.DefaultPath);
        lookups.ShouldBe(1);
    }

    [Fact]
    public void Resolve_keeps_a_file_that_exists_and_leaves_other_urls_alone()
    {
        var configured = $"https://console.sufichain.com/api/file-manager/file-items/{MissingFileId}/download";

        BrandingLogoUrls.Resolve(configured, _ => true).ShouldBe(configured);
        BrandingLogoUrls.Resolve("/sufi-chain-logo.png", _ => false).ShouldBe("/sufi-chain-logo.png");
        BrandingLogoUrls.Resolve("  ", _ => false).ShouldBeNull();
    }

    [Fact]
    public void Cache_checks_a_missing_logo_once()
    {
        var configured = $"/api/file-manager/file-items/{MissingFileId:N}/download";
        var lookups = 0;
        var cache = new BrandingLogoUrlCache();

        var first = cache.Resolve(configured, _ =>
        {
            lookups++;
            return false;
        });
        var second = cache.Resolve(configured, _ =>
        {
            lookups++;
            return false;
        });

        first.ShouldBe(BrandingLogoUrls.DefaultPath);
        second.ShouldBe(BrandingLogoUrls.DefaultPath);
        lookups.ShouldBe(1);
    }
}
