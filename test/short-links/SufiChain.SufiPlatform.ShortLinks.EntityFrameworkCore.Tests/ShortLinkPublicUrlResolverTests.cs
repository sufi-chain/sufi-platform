using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp.MultiTenancy;
using Volo.Abp.UI.Navigation.Urls;
using Xunit;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortLinkPublicUrlResolverTests
{
    [Fact]
    public async Task Copied_url_uses_the_tenant_public_host()
    {
        var tenantId = Guid.NewGuid();
        var domains = Substitute.For<ITenantDomainStore>();
        domains.FindPrimaryHostAsync(tenantId).Returns("shop.example");
        var resolver = Resolver("https://sufichain.com", tenantId, domains);

        (await resolver.GetAsync("abc")).ShouldBe("https://shop.example/s/abc");
    }

    [Fact]
    public async Task Missing_domain_row_uses_the_primary_subdomain()
    {
        var tenantId = Guid.NewGuid();
        var domains = Substitute.For<ITenantDomainStore>();
        domains.FindPrimaryHostAsync(tenantId).Returns((string?)null);
        domains.ListPublicHostsAsync().Returns(new List<TenantPublicHost>
        {
            new()
            {
                TenantId = tenantId,
                PrimarySubdomain = "demo"
            }
        });
        var resolver = Resolver("https://{0}.sufichain.com", tenantId, domains);

        (await resolver.GetAsync("abc")).ShouldBe("https://demo.sufichain.com/s/abc");
    }

    [Fact]
    public async Task Host_without_a_tenant_does_not_use_the_console_root()
    {
        var domains = Substitute.For<ITenantDomainStore>();
        var resolver = Resolver("https://public.example", null, domains);

        (await resolver.GetAsync("abc")).ShouldBe("https://public.example/s/abc");
    }

    private static ShortLinkPublicUrlResolver Resolver(string publicRoot, Guid? tenantId, ITenantDomainStore domains)
    {
        var urls = new AppUrlOptions();
        urls.Applications[ShortLinkPublicUrl.PublicApplicationName].RootUrl = publicRoot;
        var current = Substitute.For<ICurrentTenant>();
        current.Id.Returns(tenantId);
        return new ShortLinkPublicUrlResolver(
            Options.Create(urls),
            new ConfigurationBuilder().Build(),
            current,
            domains);
    }
}
