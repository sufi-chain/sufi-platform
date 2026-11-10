using System;
using System.Threading.Tasks;
using NSubstitute;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus.Distributed;
using Xunit;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantDomainMapCacheInvalidatorTests
{
    [Fact]
    public async Task Tenant_change_removes_the_map_and_publishes_for_other_hosts()
    {
        var cache = Substitute.For<IDistributedCache<TenantDomainMapCacheItem>>();
        var bus = Substitute.For<IDistributedEventBus>();
        var handler = new TenantDomainMapCacheInvalidator(cache, bus);
        var tenant = new TestTenant(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "acme");

        await handler.HandleEventAsync(new EntityChangedEventData<Tenant>(tenant));

        await cache.Received(1).RemoveAsync(TenantDomainMapCacheItem.CacheKey, considerUow: true);
        await bus.Received(1).PublishAsync(
            Arg.Is<TenantDomainMapChangedEto>(eto => eto.TenantId == tenant.Id));
    }

    [Fact]
    public async Task Distributed_event_removes_this_host_map_without_republishing()
    {
        var cache = Substitute.For<IDistributedCache<TenantDomainMapCacheItem>>();
        var bus = Substitute.For<IDistributedEventBus>();
        var handler = new TenantDomainMapCacheInvalidator(cache, bus);

        await handler.HandleEventAsync(new TenantDomainMapChangedEto
        {
            TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            OccurredAt = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc)
        });

        await cache.Received(1).RemoveAsync(TenantDomainMapCacheItem.CacheKey, considerUow: false);
        await bus.DidNotReceive().PublishAsync(Arg.Any<TenantDomainMapChangedEto>());
    }

    private sealed class TestTenant : Tenant
    {
        public TestTenant(Guid id, string name) : base(id, name, name)
        {
        }
    }
}
