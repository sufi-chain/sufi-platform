using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareRagProjectBindingCacheTests
{
    [Fact]
    public async Task Resolver_Should_Use_The_Cache_Until_It_Is_Invalidated()
    {
        var tenantId = Guid.NewGuid();
        var hooshvareId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var cache = new HooshvareRagProjectBindingListCache();
        var repository = Substitute.For<IHooshvareRagProjectBindingRepository>();
        repository.GetListByHooshvareAsync(
                Arg.Any<Guid>(),
                Arg.Any<string?>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(
            [
                new HooshvareRagProjectBinding(
                    Guid.NewGuid(),
                    tenantId,
                    hooshvareId,
                    projectId,
                    HooshvareRagSourceNames.HelpDeskKnowledgeBase)
            ]);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);
        var resolver = new ProbeResolver(repository, cache, currentTenant);

        var first = await resolver.LoadAsync(hooshvareId);
        var second = await resolver.LoadAsync(hooshvareId);
        cache.Invalidate(tenantId, hooshvareId);
        var third = await resolver.LoadAsync(hooshvareId);

        first.ShouldBe([projectId]);
        second.ShouldBe([projectId]);
        third.ShouldBe([projectId]);
        await repository.Received(2).GetListByHooshvareAsync(
            hooshvareId,
            HooshvareRagSourceNames.HelpDeskKnowledgeBase,
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Invalidate_Should_Drop_Only_The_Named_Hooshvare()
    {
        var cache = new HooshvareRagProjectBindingListCache();
        var tenantId = Guid.NewGuid();
        var dropped = Guid.NewGuid();
        var kept = Guid.NewGuid();
        var keptProject = Guid.NewGuid();
        cache.SetEnabledProjectIds(tenantId, dropped, "HelpDesk.KnowledgeBase", [Guid.NewGuid()]);
        cache.SetEnabledProjectIds(tenantId, kept, "HelpDesk.KnowledgeBase", [keptProject]);

        cache.Invalidate(tenantId, dropped);

        cache.TryGetEnabledProjectIds(tenantId, dropped, "HelpDesk.KnowledgeBase", out _).ShouldBeFalse();
        cache.TryGetEnabledProjectIds(tenantId, kept, "HelpDesk.KnowledgeBase", out var remaining).ShouldBeTrue();
        remaining.ShouldBe([keptProject]);
    }

    private sealed class ProbeResolver : HooshvareEffectiveConfigurationResolver
    {
        public ProbeResolver(
            IHooshvareRagProjectBindingRepository repository,
            IHooshvareRagProjectBindingListCache cache,
            ICurrentTenant currentTenant)
            : base(null!, repository, null!, cache, currentTenant)
        {
        }

        public Task<List<Guid>> LoadAsync(Guid hooshvareId) =>
            LoadEnabledProjectIdsAsync(hooshvareId, CancellationToken.None);
    }
}
