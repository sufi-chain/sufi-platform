using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortUrlClickedHandler : IDistributedEventHandler<ShortUrlClickedEto>, ITransientDependency
{
    private readonly IShortUrlRepository _shortUrls;
    private readonly IRepository<ShortUrlClick, Guid> _clicks;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;

    public ShortUrlClickedHandler(
        IShortUrlRepository shortUrls,
        IRepository<ShortUrlClick, Guid> clicks,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator)
    {
        _shortUrls = shortUrls;
        _clicks = clicks;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
    }

    public virtual async Task HandleEventAsync(ShortUrlClickedEto eventData)
    {
        using (_currentTenant.Change(eventData.TenantId))
        {
            await _shortUrls.IncrementClickCountAsync(eventData.ShortUrlId);

            var click = new ShortUrlClick(
                _guidGenerator.Create(),
                eventData.ShortUrlId,
                eventData.ClickedAt,
                eventData.UserAgent,
                eventData.IpAddress,
                eventData.Referrer)
            {
                TenantId = eventData.TenantId,
                Token = eventData.Token,
                DedupKey = eventData.DedupKey
            };

            await _clicks.InsertAsync(click, autoSave: true);
        }
    }
}
