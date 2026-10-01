using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.ShortLinks;
using SufiChain.SufiPlatform.ShortLinks.Hooshvare;
using SufiChain.SufiPlatform.ShortLinks.Permissions;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.ShortLinks.Mcp;

[Authorize]
public class ShortLinksAnalyticsMcpAppService : SufiApplicationService
{
    private const int MaxResults = 50;

    protected IShortUrlAppService ShortUrls { get; }

    public ShortLinksAnalyticsMcpAppService(IShortUrlAppService shortUrls)
    {
        ShortUrls = shortUrls;
    }

    [SufiAiMcpTool(ShortLinksAnalyticsHooshvareKeys.Tools.List,
        "Lists short links. Destination URLs omit query strings.", ReadOnly = true)]
    [Authorize(ShortLinksPermissions.ShortLinks.Default)]
    public virtual async Task<object> ListAsync(string? filter = null, string? shortCode = null)
    {
        var page = await ShortUrls.GetListAsync(new GetShortUrlListDto
        {
            Filter = string.IsNullOrWhiteSpace(shortCode) ? filter : shortCode,
            MaxResultCount = MaxResults
        });
        return new
        {
            page.TotalCount,
            Items = page.Items.Select(MapLink).ToList()
        };
    }

    [SufiAiMcpTool(ShortLinksAnalyticsHooshvareKeys.Tools.Get,
        "Returns one short link. The destination URL omits its query string.", ReadOnly = true)]
    [Authorize(ShortLinksPermissions.ShortLinks.Default)]
    public virtual async Task<object> GetAsync(Guid? id = null, string? shortCode = null)
    {
        if (id.HasValue)
        {
            return MapLink(await ShortUrls.GetAsync(id.Value));
        }

        if (!string.IsNullOrWhiteSpace(shortCode))
        {
            return MapLink(await ShortUrls.GetByShortCodeAsync(shortCode));
        }

        return new { Error = "id or shortCode is required" };
    }

    [SufiAiMcpTool(ShortLinksAnalyticsHooshvareKeys.Tools.GetAnalytics,
        "Returns click counts and recent clicks without IP addresses.", ReadOnly = true)]
    [Authorize(ShortLinksPermissions.ShortLinks.ViewAnalytics)]
    public virtual async Task<object> GetAnalyticsAsync(Guid id)
    {
        var analytics = await ShortUrls.GetAnalyticsAsync(id);
        return new
        {
            analytics.Id,
            analytics.ShortCode,
            analytics.ClickCount,
            analytics.LastAccessedAt,
            RecentClicks = analytics.RecentClicks.Select(click => new
            {
                click.ClickedAt,
                click.UserAgent,
                click.Referrer
            }).ToList()
        };
    }

    private static object MapLink(ShortUrlDto link) => new
    {
        link.Id,
        link.ShortCode,
        DestinationUrl = StripQuery(link.DestinationUrl),
        link.ClickCount,
        link.LastAccessedAt,
        link.IsActive,
        link.ExpiresAt,
        link.CreatedByModule,
        link.Description
    };

    private static string StripQuery(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        var query = url.IndexOf('?');
        return query < 0 ? url : url[..query];
    }
}
