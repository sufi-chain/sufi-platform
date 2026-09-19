using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Knowledge;
using SufiChain.SufiPlatform.Tags.Features;
using SufiChain.SufiPlatform.Tags.Permissions;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class KnowledgeRelationReviewAppServiceTests
{
    private static readonly DateTime Now = new(2026, 9, 16, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Create_Should_Bind_Evidence_And_Ignore_Client_Identities()
    {
        var fixture = new Fixture();
        fixture.User.Id.Returns(fixture.Proposer);
        var result = await fixture.Service().CreateAsync(fixture.CreateInput());
        result.State.ShouldBe(KnowledgeProposalState.Pending);
        result.ApplicationState.ShouldBeNull();
        result.ReviewerId.ShouldBeNull();
        await fixture.Proposals.Received(1).InsertAsync(Arg.Is<KnowledgeRelationProposal>(p =>
            p.ProposedBy == fixture.Proposer && p.Id != Guid.Empty && p.RequestId != Guid.Empty &&
            p.EvidenceSha256 == new string('a', 64)), true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Model_Caller_Cannot_Approve_Own_Proposal_Or_Forge_Reviewer()
    {
        var fixture = new Fixture();
        fixture.User.Id.Returns(fixture.Proposer);
        var proposal = fixture.StoredProposal();
        fixture.Proposals.FindAsync(proposal.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(proposal);
        (await Should.ThrowAsync<BusinessException>(() => fixture.Service().ApproveAsync(fixture.ReviewInput(proposal))))
            .Code.ShouldBe(AIErrorCodes.KnowledgeReviewDenied);
        await fixture.Intents.DidNotReceive().InsertAsync(Arg.Any<KnowledgeRelationApplicationIntent>(), Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
        (await Should.ThrowAsync<BusinessException>(() => fixture.Service().RejectAsync(fixture.ReviewInput(proposal))))
            .Code.ShouldBe(AIErrorCodes.KnowledgeReviewDenied);
    }

    [Fact]
    public async Task Approve_Creates_Pending_Apply_Intent_Without_Claiming_Tags_Success()
    {
        var fixture = new Fixture();
        var proposal = fixture.StoredProposal();
        fixture.Proposals.FindAsync(proposal.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(proposal);
        var result = await fixture.Service().ApproveAsync(fixture.ReviewInput(proposal));
        result.State.ShouldBe(KnowledgeProposalState.Approved);
        result.ApplicationState.ShouldBe(KnowledgeApplicationIntentState.PendingApply);
        result.AppliedRelationVersion.ShouldBeNull();
        result.ReviewerId.ShouldBe(fixture.Reviewer);
        await fixture.Commands.DidNotReceive().ApplyAsync(Arg.Any<TagRelationMutationRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryApply_Reconciles_Lost_Ack_From_Durable_Receipt()
    {
        var fixture = new Fixture();
        var proposal = fixture.StoredProposal();
        proposal.Approve(1, fixture.Reviewer, Now, Now.AddHours(1), "Reviewed");
        var intent = new KnowledgeRelationApplicationIntent(proposal, Now);
        fixture.Intents.FindAsync(intent.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(intent);
        fixture.Proposals.FindAsync(proposal.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(proposal);
        fixture.Commands.FindReceiptAsync(intent.Id, Arg.Any<CancellationToken>()).Returns(
            new TagRelationMutationReceiptRecord(intent.Id, proposal.RelationId, 1, proposal.Action, proposal.Id, Now));
        var result = await fixture.Service().TryApplyAsync(intent.Id);
        result.ApplicationState.ShouldBe(KnowledgeApplicationIntentState.Applied);
        result.AppliedRelationVersion.ShouldBe(1);
        await fixture.Commands.DidNotReceive().ApplyAsync(Arg.Any<TagRelationMutationRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revocation_Racing_Delayed_Apply_Does_Not_Mutate_Tags()
    {
        var fixture = new Fixture();
        var approved = fixture.StoredProposal();
        approved.Approve(1, fixture.Reviewer, Now, Now.AddHours(1), "Reviewed");
        var intent = new KnowledgeRelationApplicationIntent(approved, Now);
        approved.Revoke(2, fixture.Reviewer, Now.AddMinutes(1), "Withdrawn");
        fixture.Intents.FindAsync(intent.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(intent);
        fixture.Proposals.FindAsync(approved.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(approved);
        fixture.Proposals.FindForVerificationAsync(approved.Id, fixture.TenantId, Arg.Any<CancellationToken>()).Returns(approved);
        fixture.Commands.FindReceiptAsync(intent.Id, Arg.Any<CancellationToken>())
            .Returns((TagRelationMutationReceiptRecord?)null);
        var result = await fixture.Service().TryApplyAsync(intent.Id);
        result.ApplicationState.ShouldBe(KnowledgeApplicationIntentState.NeedsReview);
        await fixture.Commands.DidNotReceive().ApplyAsync(Arg.Any<TagRelationMutationRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Expired_Approval_Does_Not_Apply_And_Does_Not_Claim_Success()
    {
        var fixture = new Fixture();
        var approved = fixture.StoredProposal();
        approved.Approve(1, fixture.Reviewer, Now, Now.AddHours(1), "Reviewed");
        var intent = new KnowledgeRelationApplicationIntent(approved, Now);
        fixture.Clock.Now.Returns(Now.AddHours(2));
        fixture.Intents.FindAsync(intent.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(intent);
        fixture.Proposals.FindAsync(approved.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(approved);
        fixture.Proposals.FindForVerificationAsync(approved.Id, fixture.TenantId, Arg.Any<CancellationToken>()).Returns(approved);
        fixture.Commands.FindReceiptAsync(intent.Id, Arg.Any<CancellationToken>())
            .Returns((TagRelationMutationReceiptRecord?)null);
        var result = await fixture.Service().TryApplyAsync(intent.Id);
        result.ApplicationState.ShouldBe(KnowledgeApplicationIntentState.NeedsReview);
        await fixture.Commands.DidNotReceive().ApplyAsync(Arg.Any<TagRelationMutationRequest>(), Arg.Any<CancellationToken>());
    }

    private sealed class Fixture
    {
        public Guid TenantId { get; } = Guid.NewGuid();
        public Guid Proposer { get; } = Guid.NewGuid();
        public Guid Reviewer { get; } = Guid.NewGuid();
        public IKnowledgeRelationProposalRepository Proposals { get; } = Substitute.For<IKnowledgeRelationProposalRepository>();
        public IKnowledgeRelationApplicationIntentRepository Intents { get; } = Substitute.For<IKnowledgeRelationApplicationIntentRepository>();
        public IPermissionChecker Permissions { get; } = Substitute.For<IPermissionChecker>();
        public IFeatureChecker Features { get; } = Substitute.For<IFeatureChecker>();
        public ICurrentUser User { get; } = Substitute.For<ICurrentUser>();
        public ICurrentTenant Tenant { get; } = Substitute.For<ICurrentTenant>();
        public IGuidGenerator Ids { get; } = Substitute.For<IGuidGenerator>();
        public IClock Clock { get; } = Substitute.For<IClock>();
        public IKnowledgeProposalEvidenceValidator Evidence { get; } = Substitute.For<IKnowledgeProposalEvidenceValidator>();
        public ITagRelationEndpointAccess Endpoints { get; } = Substitute.For<ITagRelationEndpointAccess>();
        public ITagRelationMutationCoordinator Commands { get; } = Substitute.For<ITagRelationMutationCoordinator>();

        public Fixture()
        {
            Tenant.Id.Returns(TenantId);
            User.Id.Returns(Reviewer);
            User.IsAuthenticated.Returns(true);
            Permissions.IsGrantedAsync(Arg.Any<string>()).Returns(true);
            Features.IsEnabledAsync(Arg.Any<string>()).Returns(true);
            Ids.Create().Returns(_ => Guid.NewGuid());
            Clock.Now.Returns(Now);
            Evidence.IsCurrentAndAccessibleAsync(Arg.Any<KnowledgeProposalEvidence>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(true);
            Endpoints.CanRelateInScopeAsync(Arg.Any<TagEntityReference>(), Arg.Any<TagEntityReference>(),
                Arg.Any<TagEntityReference>(), Arg.Any<CancellationToken>()).Returns(true);
            Intents.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns((KnowledgeRelationApplicationIntent?)null);
            Intents.InsertAsync(Arg.Any<KnowledgeRelationApplicationIntent>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    var intent = call.Arg<KnowledgeRelationApplicationIntent>();
                    Intents.FindAsync(intent.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(intent);
                    return intent;
                });
        }

        public CreateKnowledgeRelationProposalDto CreateInput()
        {
            var request = Request();
            return new CreateKnowledgeRelationProposalDto
            {
                RelationId = request.RelationId,
                DefinitionId = request.DefinitionId,
                ExpectedRelationVersion = 0,
                Action = TagRelationMutationKind.Create,
                ScopeType = request.Scope.EntityType,
                ScopeId = request.Scope.EntityId,
                SourceType = request.Source.EntityType,
                SourceId = request.Source.EntityId,
                TargetType = request.Target.EntityType,
                TargetId = request.Target.EntityId,
                ValidFromUtc = request.ValidFromUtc,
                Reason = request.Reason,
                EvidenceType = request.Source.EntityType,
                EvidenceId = request.Source.EntityId,
                EvidenceVersion = "published-v3",
                EvidenceSha256 = new string('a', 64),
                EvidenceLocator = "utf16:0:64:en"
            };
        }

        public KnowledgeRelationProposal StoredProposal()
        {
            var request = Request();
            return new KnowledgeRelationProposal(Guid.NewGuid(), 1, TenantId, request, request.Source,
                "published-v3", new string('a', 64), "utf16:0:64:en", Proposer, Now);
        }

        public ReviewKnowledgeRelationDto ReviewInput(KnowledgeRelationProposal proposal) => new()
        {
            DecisionId = proposal.Id,
            ExpectedVersion = proposal.Version,
            Reason = "Human reviewed the published passage.",
            ExpiresAtUtc = Now.AddHours(1)
        };

        public static TagRelationMutationRequest Request() => new(Guid.NewGuid(), Guid.NewGuid(), 0,
            TagRelationMutationKind.Create, new("helpdesk.project", Guid.NewGuid()),
            new("helpdesk.article", Guid.NewGuid()), new("helpdesk.ticket", Guid.NewGuid()),
            Now.AddTicks(123), null, Guid.NewGuid(), Guid.NewGuid(), "Documented resolution");

        public KnowledgeRelationReviewAppService Service() => new(Proposals, Intents, Permissions, Features, User, Tenant,
            Ids, Clock, new[] { Evidence }, new[] { Endpoints }, new[] { Commands });
    }
}
