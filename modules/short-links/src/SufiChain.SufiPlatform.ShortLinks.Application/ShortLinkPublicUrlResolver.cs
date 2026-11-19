using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.UI.Navigation.Urls;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortLinkPublicUrlResolver : ITransientDependency
{
    private readonly IOptions<AppUrlOptions> _appUrls;
    private readonly IConfiguration _configuration;
    private readonly ICurrentTenant _currentTenant;
    private readonly ITenantDomainStore _domains;

    public ShortLinkPublicUrlResolver(
        IOptions<AppUrlOptions> appUrls,
        IConfiguration configuration,
        ICurrentTenant currentTenant,
        ITenantDomainStore domains)
    {
        _appUrls = appUrls;
        _configuration = configuration;
        _currentTenant = currentTenant;
        _domains = domains;
    }

    public virtual async Task<string> GetAsync(string shortCode)
    {
        string? tenantHost = null;
        string? tenantSubdomain = null;
        if (_currentTenant.Id.HasValue)
        {
            tenantHost = await _domains.FindPrimaryHostAsync(_currentTenant.Id.Value);
            if (string.IsNullOrWhiteSpace(tenantHost))
            {
                var hosts = await _domains.ListPublicHostsAsync();
                tenantSubdomain = hosts.FirstOrDefault(host => host.TenantId == _currentTenant.Id)?.PrimarySubdomain;
            }
        }

        var publicRoot = _appUrls.Value.Applications[ShortLinkPublicUrl.PublicApplicationName]?.RootUrl;
        var origin = ShortLinkPublicUrl.ResolveOrigin(
            publicRoot,
            _configuration["App:PublicHostUrl"],
            _configuration["OpenIddict:Applications:SufiPlatform_PublicHost:RootUrl"],
            tenantHost,
            tenantSubdomain);
        return ShortLinkPublicUrl.Build(origin, shortCode);
    }
}
