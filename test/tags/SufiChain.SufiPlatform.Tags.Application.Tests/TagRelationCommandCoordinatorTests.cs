using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Tags.Relations;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.Tags;

public class TagRelationCommandCoordinatorTests
{
    [Fact]
    public async Task Apply_Should_Persist_Receipt_With_Stable_Request_Identity()
    {
        var fixture = new Fixture();
        var request = fixture.Request();
        var receipt = await fixture.Coordinator().ApplyAsync(request);
        receipt.RequestId.ShouldBe(request.RequestId);
        receipt.RelationId.ShouldBe(request.RelationId);
        receipt.AppliedVersion.ShouldBe(1);
        await fixture.Relations.Received(1).InsertAsync(Arg.Is<EntityRelation>(x => x.Id == request.RelationId), true,
            Arg.Any<CancellationToken>());
        await fixture.Receipts.Received(1).InsertAsync(Arg.Is<TagRelationMutationReceipt>(x => x.Id == request.RequestId),
            true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Retry_Returns_Existing_Receipt_Without_A_Second_Mutation()
    {
        var fixture = new Fixture();
        var request = fixture.Request();
        var existing = new TagRelationMutationReceipt(request, fixture.TenantId, 1, Fixture.Now);
        fixture.Receipts.FindAsync(request.RequestId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(existing);
        var receipt = await fixture.Coordinator().ApplyAsync(request);
        receipt.AppliedVersion.ShouldBe(1);
        await fixture.Relations.DidNotReceive().InsertAsync(Arg.Any<EntityRelation>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cycle_Guarded_Writes_Remain_Denied()
    {
        var fixture = new Fixture(acyclic: true);
        (await Should.ThrowAsync<BusinessException>(() => fixture.Coordinator().ApplyAsync(fixture.Request())))
            .Code.ShouldBe(TagsErrorCodes.RelationCommandDenied);
        await fixture.Receipts.DidNotReceive().InsertAsync(Arg.Any<TagRelationMutationReceipt>(), Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    private sealed class Fixture
    {
        public static readonly DateTime Now = new(2026, 9, 16, 19, 0, 0, DateTimeKind.Utc);
        public Guid TenantId { get; } = Guid.NewGuid();
        public Guid ActorId { get; } = Guid.NewGuid();
        public IRepository<TagRelationMutationReceipt, Guid> Receipts { get; } =
            Substitute.For<IRepository<TagRelationMutationReceipt, Guid>>();
        public IEntityRelationRepository Relations { get; } = Substitute.For<IEntityRelationRepository>();
        public ICurrentUser User { get; } = Substitute.For<ICurrentUser>();
        public ICurrentTenant Tenant { get; } = Substitute.For<ICurrentTenant>();
        public IClock Clock { get; } = Substitute.For<IClock>();
        public ITagRelationApprovalVerifier Approval { get; } = Substitute.For<ITagRelationApprovalVerifier>();
        private IRepository<TagRelationDefinition, Guid> Definitions { get; } = Substitute.For<IRepository<TagRelationDefinition, Guid>>();
        private ITagRepository Tags { get; } = Substitute.For<ITagRepository>();
        private TagEntityReference Scope { get; } = new("helpdesk.project", Guid.NewGuid());
        private TagRelationDefinition Definition { get; }
        private Tag Predicate { get; }
        private TagRelationEndpointPolicy Endpoints { get; }

        public Fixture(bool acyclic = false)
        {
            Tenant.Id.Returns(TenantId);
            User.Id.Returns(ActorId);
            User.IsAuthenticated.Returns(true);
            Clock.Now.Returns(Now);
            Predicate = Tag.CreateRelationPredicate(Guid.NewGuid(), "Related", "helpdesk.relations", "related-article", TenantId);
            Definition = new TagRelationDefinition(Guid.NewGuid(), TenantId, Predicate.Id, "related-article", 1,
                "helpdesk.article", "helpdesk.article", isSymmetric: !acyclic, requiresAcyclicGraph: acyclic);
            Definitions.FindAsync(Definition.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Definition);
            Definitions.GetAsync(Definition.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Definition);
            Tags.FindAsync(Predicate.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Predicate);
            Approval.IsApprovedAsync(Arg.Any<TagRelationMutationRequest>(), Arg.Any<Guid?>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
            Receipts.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns((TagRelationMutationReceipt?)null);
            Relations.FindByRequestIdAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns((EntityRelation?)null);
            var resolver = Substitute.For<ITagRelationEntityResolver>();
            resolver.EntityType.Returns("helpdesk.article");
            resolver.ResolveAsync(Arg.Any<IReadOnlyList<TagEntityReference>>(), Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<IReadOnlyList<TagEntityReference>>()
                    .Select(reference => new TagRelationEntityResolution(reference, TenantId, Scope, true, true)).ToArray());
            Endpoints = new TagRelationEndpointPolicy(new[] { resolver }, Tenant);
        }

        public TagRelationMutationRequest Request() => new(Guid.NewGuid(), Definition.Id, 0,
            TagRelationMutationKind.Create, Scope,
            new TagEntityReference("helpdesk.article", Guid.NewGuid()),
            new TagEntityReference("helpdesk.article", Guid.NewGuid()),
            null, null, Guid.NewGuid(), Guid.NewGuid(), "Human reviewed");

        public TagRelationCommandCoordinator Coordinator()
        {
            var permissions = Substitute.For<Volo.Abp.Authorization.Permissions.IPermissionChecker>();
            var features = Substitute.For<Volo.Abp.Features.IFeatureChecker>();
            permissions.IsGrantedAsync(Arg.Any<string>()).Returns(true);
            features.IsEnabledAsync(Arg.Any<string>()).Returns(true);
            var policy = new TagRelationCommandPolicy(Tenant, User, permissions, features, Endpoints, Definitions, Tags,
                Relations, new[] { Approval });
            return new TagRelationCommandCoordinator(policy, Definitions, Relations, Receipts, User, Tenant, Clock);
        }
    }
}
