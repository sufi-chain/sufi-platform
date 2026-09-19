using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Tags.Features;
using SufiChain.SufiPlatform.Tags.Permissions;
using SufiChain.SufiPlatform.Tags.Relations;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.Tags;

public class TagRelationCommandPolicyTests
{
    [Fact]
    public async Task Should_Verify_Exact_Payload_And_Trusted_Identity_Without_Writing()
    {
        var fixture = new Fixture();
        await fixture.Policy().ValidateAsync(fixture.Request());
        await fixture.Approval.Received(1).IsApprovedAsync(Arg.Any<TagRelationMutationRequest>(), fixture.TenantId,
            fixture.ActorId, Arg.Any<CancellationToken>());
        await fixture.Relations.DidNotReceive().InsertAsync(Arg.Any<EntityRelation>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("anonymous")]
    [InlineData("permission")]
    [InlineData("feature")]
    [InlineData("scope")]
    [InlineData("approval")]
    [InlineData("missing-verifier")]
    [InlineData("duplicate-verifier")]
    [InlineData("deleted-predicate")]
    [InlineData("foreign-definition")]
    public async Task Should_Deny_Unsafe_Commands(string failure)
    {
        var fixture = new Fixture();
        var request = fixture.Request(foreignScope: failure == "scope");
        if (failure == "anonymous") fixture.User.IsAuthenticated.Returns(false);
        if (failure == "permission") fixture.Permissions.IsGrantedAsync(TagsPermissions.Relations.Mutate).Returns(false);
        if (failure == "feature") fixture.Features.IsEnabledAsync(SufiTagsFeatures.Relations).Returns(false);
        if (failure == "approval") fixture.Approval.IsApprovedAsync(Arg.Any<TagRelationMutationRequest>(), Arg.Any<Guid?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        if (failure == "deleted-predicate") fixture.Predicate.IsDeleted = true;
        if (failure == "foreign-definition") fixture.Tenant.Id.Returns(Guid.NewGuid());
        var count = failure == "missing-verifier" ? 0 : failure == "duplicate-verifier" ? 2 : 1;
        (await Should.ThrowAsync<BusinessException>(() => fixture.Policy(count).ValidateAsync(request))).Code
            .ShouldBe(TagsErrorCodes.RelationCommandDenied);
    }

    [Fact]
    public async Task Should_Deny_Cycle_Guarded_Create_Until_Transactional_Graph_Checks_Exist()
    {
        var fixture = new Fixture(acyclic: true);
        (await Should.ThrowAsync<BusinessException>(() => fixture.Policy().ValidateAsync(fixture.Request()))).Code
            .ShouldBe(TagsErrorCodes.RelationCommandDenied);
        await fixture.Approval.DidNotReceive().IsApprovedAsync(Arg.Any<TagRelationMutationRequest>(), Arg.Any<Guid?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Recheck_Tenant_After_Approval_Verification()
    {
        var fixture = new Fixture();
        fixture.Approval.IsApprovedAsync(Arg.Any<TagRelationMutationRequest>(), Arg.Any<Guid?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(_ => { fixture.Tenant.Id.Returns(Guid.NewGuid()); return true; });
        await Should.ThrowAsync<BusinessException>(() => fixture.Policy().ValidateAsync(fixture.Request()));
    }

    private sealed class Fixture
    {
        public Guid TenantId { get; } = Guid.NewGuid();
        public Guid ActorId { get; } = Guid.NewGuid();
        public ICurrentTenant Tenant { get; } = Substitute.For<ICurrentTenant>();
        public ICurrentUser User { get; } = Substitute.For<ICurrentUser>();
        public IPermissionChecker Permissions { get; } = Substitute.For<IPermissionChecker>();
        public IFeatureChecker Features { get; } = Substitute.For<IFeatureChecker>();
        public ITagRelationApprovalVerifier Approval { get; } = Substitute.For<ITagRelationApprovalVerifier>();
        public IEntityRelationRepository Relations { get; } = Substitute.For<IEntityRelationRepository>();
        public Tag Predicate { get; }
        private IRepository<TagRelationDefinition, Guid> Definitions { get; } = Substitute.For<IRepository<TagRelationDefinition, Guid>>();
        private ITagRepository Tags { get; } = Substitute.For<ITagRepository>();
        private TagEntityReference Scope { get; } = new("helpdesk.project", Guid.NewGuid());
        private TagRelationDefinition Definition { get; }
        private TagRelationEndpointPolicy Endpoints { get; }

        public Fixture(bool acyclic = false)
        {
            Tenant.Id.Returns(TenantId);
            User.Id.Returns(ActorId);
            User.IsAuthenticated.Returns(true);
            Permissions.IsGrantedAsync(Arg.Any<string>()).Returns(true);
            Features.IsEnabledAsync(Arg.Any<string>()).Returns(true);
            Predicate = Tag.CreateRelationPredicate(Guid.NewGuid(), "Related", "helpdesk.relations", "related-article", TenantId);
            Definition = new TagRelationDefinition(Guid.NewGuid(), TenantId, Predicate.Id, "related-article", 1,
                "helpdesk.article", "helpdesk.article", isSymmetric: !acyclic, requiresAcyclicGraph: acyclic);
            Definitions.FindAsync(Definition.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Definition);
            Tags.FindAsync(Predicate.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Predicate);
            Approval.IsApprovedAsync(Arg.Any<TagRelationMutationRequest>(), Arg.Any<Guid?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
            var resolver = Substitute.For<ITagRelationEntityResolver>();
            resolver.EntityType.Returns("helpdesk.article");
            resolver.ResolveAsync(Arg.Any<IReadOnlyList<TagEntityReference>>(), Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<IReadOnlyList<TagEntityReference>>()
                    .Select(reference => new TagRelationEntityResolution(reference, TenantId, Scope, true, true)).ToArray());
            Endpoints = new TagRelationEndpointPolicy(new[] { resolver }, Tenant);
        }

        public TagRelationMutationRequest Request(bool foreignScope = false) => new(Guid.NewGuid(), Definition.Id, 0,
            TagRelationMutationKind.Create, foreignScope ? new TagEntityReference("helpdesk.project", Guid.NewGuid()) : Scope,
            new TagEntityReference("helpdesk.article", Guid.NewGuid()), new TagEntityReference("helpdesk.article", Guid.NewGuid()),
            null, null, Guid.NewGuid(), Guid.NewGuid(), "Human reviewed");

        public TagRelationCommandPolicy Policy(int verifierCount = 1) => new(Tenant, User, Permissions, Features,
            Endpoints, Definitions, Tags, Relations, Enumerable.Repeat(Approval, verifierCount));
    }
}
