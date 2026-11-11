using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortUrlClickedHandlerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Records_The_Click_In_The_Link_Tenant(bool hostLink)
    {
        Guid? tenantId = hostLink ? null : Guid.NewGuid();
        var shortUrlId = Guid.NewGuid();
        var clickedAt = new DateTime(2026, 11, 11, 9, 0, 0, DateTimeKind.Utc);
        var shortUrls = Substitute.For<IShortUrlRepository>();
        shortUrls.IncrementClickCountAsync(shortUrlId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var clicks = Substitute.For<IRepository<ShortUrlClick, Guid>>();
        clicks.InsertAsync(Arg.Any<ShortUrlClick>(), true, Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<ShortUrlClick>()));
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Change(tenantId).Returns(Substitute.For<IDisposable>());
        var handler = new ShortUrlClickedHandler(
            shortUrls,
            clicks,
            currentTenant,
            SimpleGuidGenerator.Instance);

        await handler.HandleEventAsync(new ShortUrlClickedEto
        {
            ShortUrlId = shortUrlId,
            TenantId = tenantId,
            ClickedAt = clickedAt,
            UserAgent = "qa",
            IpAddress = "127.0.0.1",
            DedupKey = $"{shortUrlId}:127.0.0.1"
        });

        currentTenant.Received(1).Change(tenantId);
        await shortUrls.Received(1).IncrementClickCountAsync(shortUrlId, Arg.Any<CancellationToken>());
        await clicks.Received(1).InsertAsync(
            Arg.Is<ShortUrlClick>(click =>
                click.ShortUrlId == shortUrlId &&
                click.TenantId == tenantId &&
                click.ClickedAt == clickedAt &&
                click.DedupKey == $"{shortUrlId}:127.0.0.1"),
            true,
            Arg.Any<CancellationToken>());
    }
}
