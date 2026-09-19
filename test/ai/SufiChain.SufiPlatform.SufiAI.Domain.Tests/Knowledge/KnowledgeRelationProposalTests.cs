using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Knowledge;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class KnowledgeRelationProposalTests
{
    private static readonly DateTime Now = new(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _reviewer = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();

    public static TagRelationMutationRequest Request() => new(Guid.NewGuid(), Guid.NewGuid(), 0,
        TagRelationMutationKind.Create, new("helpdesk.project", Guid.NewGuid()),
        new("helpdesk.article", Guid.NewGuid()), new("helpdesk.ticket", Guid.NewGuid()),
        Now.AddTicks(123), null, Guid.NewGuid(), Guid.NewGuid(), "Documented resolution");

    public static KnowledgeRelationProposal Proposal(TagRelationMutationRequest request, Guid? tenantId = null,
        Guid? proposalId = null, int version = 1) => new(proposalId ?? Guid.NewGuid(), version, tenantId,
        request, request.Source, "published-v3", new string('a', 64), "utf16:0:64", Guid.NewGuid(), Now);

    [Fact]
    public void Approval_Must_Bind_Exact_Payload_Actor_Tenant_And_Expiry()
    {
        var request = Request();
        var proposal = Proposal(request, _tenantId);
        proposal.Authorizes(request, _tenantId, _reviewer, Now).ShouldBeFalse();
        proposal.Approve(1, _reviewer, Now, Now.AddHours(1), "Verified source");
        proposal.Authorizes(request, _tenantId, _reviewer, Now).ShouldBeTrue();
        proposal.Authorizes(request, null, _reviewer, Now).ShouldBeFalse();
        proposal.Authorizes(request, _tenantId, Guid.NewGuid(), Now).ShouldBeFalse();
        proposal.Authorizes(request, _tenantId, _reviewer, Now.AddHours(1)).ShouldBeFalse();
        proposal.Authorizes(request, _tenantId, _reviewer, Now.AddTicks(-1)).ShouldBeFalse();
    }

    [Theory]
    [InlineData("relation")]
    [InlineData("definition")]
    [InlineData("action")]
    [InlineData("scope")]
    [InlineData("source")]
    [InlineData("target")]
    [InlineData("from")]
    [InlineData("to")]
    [InlineData("approval")]
    [InlineData("request")]
    [InlineData("reason")]
    public void Approval_Rejects_Changed_Command(string field)
    {
        var request = Request();
        var proposal = Proposal(request);
        proposal.Approve(1, _reviewer, Now, Now.AddHours(1), "Reviewed");
        var changed = new TagRelationMutationRequest(
            field == "relation" ? Guid.NewGuid() : request.RelationId,
            field == "definition" ? Guid.NewGuid() : request.DefinitionId,
            field == "action" ? 1 : request.ExpectedVersion,
            field == "action" ? TagRelationMutationKind.Revise : request.Action,
            field == "scope" ? new("helpdesk.project", Guid.NewGuid()) : request.Scope,
            field == "source" ? new("helpdesk.article", Guid.NewGuid()) : request.Source,
            field == "target" ? new("helpdesk.ticket", Guid.NewGuid()) : request.Target,
            field == "from" ? request.ValidFromUtc!.Value.AddTicks(1) : request.ValidFromUtc,
            field == "to" ? Now.AddDays(1) : request.ValidToUtc,
            field == "approval" ? Guid.NewGuid() : request.ApprovalDecisionId,
            field == "request" ? Guid.NewGuid() : request.RequestId,
            field == "reason" ? "Different reason" : request.Reason);
        proposal.Authorizes(changed, null, _reviewer, Now).ShouldBeFalse();
    }

    [Fact]
    public void Rejected_And_Revoked_Versions_Cannot_Be_Approved_Again()
    {
        var request = Request();
        var rejected = Proposal(request);
        rejected.Reject(1, _reviewer, Now, "Unsupported by evidence");
        Should.Throw<Volo.Abp.BusinessException>(() => rejected.Approve(2, _reviewer, Now, Now.AddHours(1), "Retry"));
        var approved = Proposal(Request());
        approved.Approve(1, _reviewer, Now, Now.AddHours(1), "Reviewed");
        approved.Revoke(2, _reviewer, Now.AddMinutes(1), "Source withdrawn");
        approved.ReviewReason.ShouldBe("Reviewed");
        approved.RevocationReason.ShouldBe("Source withdrawn");
        approved.Version.ShouldBe(3);
        Should.Throw<Volo.Abp.BusinessException>(() => approved.Approve(3, _reviewer, Now, Now.AddHours(1), "Retry"));
    }

    [Fact]
    public void Invalid_Review_Must_Not_Partially_Change_State()
    {
        var proposal = Proposal(Request());
        Should.Throw<Volo.Abp.BusinessException>(() => proposal.Approve(2, _reviewer, Now, Now.AddHours(1), "Stale"));
        Should.Throw<Volo.Abp.BusinessException>(() => proposal.Approve(1, Guid.Empty, Now, Now.AddHours(1), "Missing actor"));
        Should.Throw<Volo.Abp.BusinessException>(() => proposal.Approve(1, _reviewer, Now, Now.AddHours(25), "Too long"));
        Should.Throw<ArgumentException>(() => proposal.Approve(1, _reviewer, Now, Now.AddHours(1), " "));
        proposal.State.ShouldBe(KnowledgeProposalState.Pending);
        proposal.ReviewerId.ShouldBeNull();
        proposal.Version.ShouldBe(1);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("pending")]
    [InlineData("rejected")]
    [InlineData("revoked")]
    [InlineData("expired")]
    [InlineData("foreign")]
    [InlineData("context")]
    [InlineData("superseded")]
    [InlineData("evidence")]
    [InlineData("unconfigured")]
    [InlineData("duplicate")]
    [InlineData("during-validation")]
    public async Task Verifier_Should_Fail_Closed(string failure)
    {
        var request = Request();
        var proposal = Proposal(request, failure == "foreign" ? Guid.NewGuid() : _tenantId);
        if (failure == "rejected") proposal.Reject(1, _reviewer, Now, "Rejected");
        else if (failure != "pending") proposal.Approve(1, _reviewer, Now, Now.AddHours(1), "Reviewed");
        if (failure == "revoked") proposal.Revoke(2, _reviewer, Now, "Withdrawn");
        var repository = Substitute.For<IKnowledgeRelationProposalRepository>();
        repository.FindForVerificationAsync(request.ApprovalDecisionId, _tenantId, Arg.Any<CancellationToken>())
            .Returns(failure == "missing" ? null : proposal);
        repository.HasLaterVersionAsync(_tenantId, proposal.ProposalId, 1, Arg.Any<CancellationToken>())
            .Returns(failure == "superseded");
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns(failure == "context" ? null : _tenantId);
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(failure == "expired" ? Now.AddHours(1) : Now);
        var evidence = Substitute.For<IKnowledgeProposalEvidenceValidator>();
        evidence.IsCurrentAndAccessibleAsync(Arg.Any<KnowledgeProposalEvidence>(), _reviewer, Arg.Any<CancellationToken>()).Returns(_ =>
        {
            if (failure == "during-validation") proposal.Revoke(2, _reviewer, Now, "Concurrent revoke");
            return Task.FromResult(failure != "evidence");
        });
        var validators = failure == "unconfigured" ? Array.Empty<IKnowledgeProposalEvidenceValidator>() :
            failure == "duplicate" ? new[] { evidence, evidence } : new[] { evidence };
        var verifier = new KnowledgeRelationApprovalVerifier(repository, validators, tenant, clock);
        (await verifier.IsApprovedAsync(request, _tenantId, _reviewer)).ShouldBeFalse();
    }

    [Fact]
    public async Task Verifier_Should_Recheck_Durable_Record_After_Current_Evidence()
    {
        var request = Request();
        var proposal = Proposal(request, _tenantId);
        proposal.Approve(1, _reviewer, Now, Now.AddHours(1), "Reviewed");
        var repository = Substitute.For<IKnowledgeRelationProposalRepository>();
        repository.FindForVerificationAsync(request.ApprovalDecisionId, _tenantId, Arg.Any<CancellationToken>()).Returns(proposal);
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns(_tenantId);
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(Now);
        var evidence = Substitute.For<IKnowledgeProposalEvidenceValidator>();
        evidence.IsCurrentAndAccessibleAsync(Arg.Is<KnowledgeProposalEvidence>(x =>
            x.TenantId == _tenantId && x.EntityId == proposal.EvidenceId && x.Version == proposal.EvidenceVersion &&
            x.Sha256 == proposal.EvidenceSha256 && x.Locator == proposal.EvidenceLocator && x.ScopeId == proposal.ScopeId),
            _reviewer, Arg.Any<CancellationToken>()).Returns(true);
        var verifier = new KnowledgeRelationApprovalVerifier(repository, new[] { evidence }, tenant, clock);
        (await verifier.IsApprovedAsync(request, _tenantId, _reviewer)).ShouldBeTrue();
        await repository.Received(2).FindForVerificationAsync(request.ApprovalDecisionId, _tenantId, Arg.Any<CancellationToken>());
    }
}
