using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.Tags;

public class TagRelationEndpointPolicyTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly TagEntityReference _source = new("helpdesk.article", Guid.NewGuid());
    private readonly TagEntityReference _target = new("helpdesk.ticket", Guid.NewGuid());
    private readonly TagEntityReference _scope = new("helpdesk.project", Guid.NewGuid());

    [Theory]
    [InlineData("")]
    [InlineData("HelpDesk.Article")]
    [InlineData(" helpdesk.article")]
    [InlineData("helpdesk.article ")]
    [InlineData("helpdesk..article")]
    [InlineData("helpdesk.article-")]
    [InlineData("helpdesk/../../ticket")]
    [InlineData("helpdesk.مقاله")]
    public void Should_Reject_Noncanonical_Reference_Keys(string key)
    {
        Should.Throw<ArgumentException>(() => new TagEntityReference(key, Guid.NewGuid()));
    }

    [Fact]
    public void Should_Reject_Empty_Ids_And_Excessive_Keys()
    {
        Should.Throw<ArgumentException>(() => new TagEntityReference("helpdesk.article", Guid.Empty));
        Should.Throw<ArgumentException>(() => new TagEntityReference(new string('a', 97), Guid.NewGuid()));
    }

    [Fact]
    public void Should_Keep_Entity_Types_In_Reference_Identity()
    {
        var sameIdOtherType = new TagEntityReference(_target.EntityType, _source.EntityId);
        sameIdOtherType.ShouldNotBe(_source);
        new TagEntityReference(_source.EntityType, _source.EntityId).ShouldBe(_source);
    }

    [Fact]
    public async Task Should_Require_Both_Read_And_Relate_Access()
    {
        var policy = CreatePolicy(Resolve(_source), Resolve(_target, canRelate: false));
        (await policy.CanReadAsync(_source, _target)).ShouldBeTrue();
        (await policy.CanRelateAsync(_source, _target)).ShouldBeFalse();

        policy = CreatePolicy(Resolve(_source), Resolve(_target, canRead: false));
        (await policy.CanReadAsync(_source, _target)).ShouldBeFalse();
        (await policy.CanRelateAsync(_source, _target)).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Deny_Cross_Tenant_And_Host_Endpoints()
    {
        foreach (var tenantId in new Guid?[] { Guid.NewGuid(), null })
        {
            var target = new TagRelationEntityResolution(_target, tenantId, _scope, true, true);
            (await CreatePolicy(Resolve(_source), target).CanRelateAsync(_source, _target)).ShouldBeFalse();
        }
    }

    [Fact]
    public async Task Should_Deny_Cross_Project_And_Scope_Type_Mismatches()
    {
        foreach (var scope in new[]
        {
            new TagEntityReference("helpdesk.project", Guid.NewGuid()),
            new TagEntityReference("another.project", _scope.EntityId)
        })
        {
            var target = new TagRelationEntityResolution(_target, _tenantId, scope, true, true);
            (await CreatePolicy(Resolve(_source), target).CanReadAsync(_source, _target)).ShouldBeFalse();
        }
    }

    [Fact]
    public async Task Should_Deny_Unknown_And_Missing_Endpoints()
    {
        var policy = new TagRelationEndpointPolicy([Resolver(_source.EntityType, [Resolve(_source)])], Tenant());
        (await policy.CanRelateAsync(_source, _target)).ShouldBeFalse();
        policy = new TagRelationEndpointPolicy(
            [Resolver(_source.EntityType, [Resolve(_source)]), Resolver(_target.EntityType, [])], Tenant());
        (await policy.CanRelateAsync(_source, _target)).ShouldBeFalse();
    }

    [Fact]
    public void Should_Reject_Ambiguous_Adapter_Registration()
    {
        Should.Throw<InvalidOperationException>(() => new TagRelationEndpointPolicy(
            [Resolver(_source.EntityType, []), Resolver(_source.EntityType, [])], Tenant()));
    }

    [Fact]
    public async Task Should_Reject_Duplicate_Or_Unrequested_Adapter_Results()
    {
        var second = new TagEntityReference(_source.EntityType, Guid.NewGuid());
        var extra = new TagEntityReference(_source.EntityType, Guid.NewGuid());
        foreach (var results in new[]
        {
            new[] { Resolve(_source), Resolve(_source) },
            new[] { Resolve(_source), Resolve(extra) }
        })
        {
            var policy = new TagRelationEndpointPolicy([Resolver(_source.EntityType, results)], Tenant());
            (await policy.CanReadAsync(_source, second)).ShouldBeFalse();
        }
    }

    [Fact]
    public async Task Should_Batch_Same_Type_Endpoints_And_Recheck_Each_Operation()
    {
        var second = new TagEntityReference(_source.EntityType, Guid.NewGuid());
        var resolver = Resolver(_source.EntityType, [Resolve(_source), Resolve(second)]);
        var policy = new TagRelationEndpointPolicy([resolver], Tenant());
        (await policy.CanRelateAsync(_source, second)).ShouldBeTrue();
        resolver.ResolveAsync(Arg.Any<IReadOnlyList<TagEntityReference>>(), Arg.Any<CancellationToken>())
            .Returns(new List<TagRelationEntityResolution>());
        (await policy.CanRelateAsync(_source, second)).ShouldBeFalse();
        await resolver.Received(2).ResolveAsync(
            Arg.Is<IReadOnlyList<TagEntityReference>>(refs => refs.Count == 2), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Propagate_Cancellation_Without_Returning_Access()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Should.ThrowAsync<OperationCanceledException>(() =>
            CreatePolicy(Resolve(_source), Resolve(_target)).CanReadAsync(_source, _target, cancellation.Token));
    }

    private TagRelationEntityResolution Resolve(TagEntityReference reference, bool canRead = true, bool canRelate = true) =>
        new(reference, _tenantId, _scope, canRead, canRelate);

    private ICurrentTenant Tenant()
    {
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns(_tenantId);
        return tenant;
    }

    private TagRelationEndpointPolicy CreatePolicy(TagRelationEntityResolution source, TagRelationEntityResolution target) =>
        new([Resolver(source.Reference.EntityType, [source]), Resolver(target.Reference.EntityType, [target])], Tenant());

    private static ITagRelationEntityResolver Resolver(string type, IReadOnlyList<TagRelationEntityResolution> results)
    {
        var resolver = Substitute.For<ITagRelationEntityResolver>();
        resolver.EntityType.Returns(type);
        resolver.ResolveAsync(Arg.Any<IReadOnlyList<TagEntityReference>>(), Arg.Any<CancellationToken>()).Returns(results);
        return resolver;
    }
}
