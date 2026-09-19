using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Knowledge;
using SufiChain.SufiPlatform.Tags.Relations;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class KnowledgeRelationApplicationIntentTests
{
    private static readonly DateTime Now = new(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Intent_Identity_Is_The_Mutation_Request_And_Approval_Is_Not_Applied()
    {
        var request = KnowledgeRelationProposalTests.Request();
        var proposal = KnowledgeRelationProposalTests.Proposal(request, Guid.NewGuid());
        proposal.Approve(1, Guid.NewGuid(), Now, Now.AddHours(1), "Reviewed");
        var intent = new KnowledgeRelationApplicationIntent(proposal, Now);
        intent.Id.ShouldBe(request.RequestId);
        intent.State.ShouldBe(KnowledgeApplicationIntentState.PendingApply);
        intent.ToMutationRequest().RequestId.ShouldBe(request.RequestId);
        intent.AppliedRelationVersion.ShouldBeNull();
    }

    [Fact]
    public void Receipt_Is_Required_To_Mark_Applied_And_Revocation_Does_Not_Invent_Success()
    {
        var request = KnowledgeRelationProposalTests.Request();
        var proposal = KnowledgeRelationProposalTests.Proposal(request, Guid.NewGuid());
        var reviewer = Guid.NewGuid();
        proposal.Approve(1, reviewer, Now, Now.AddHours(1), "Reviewed");
        var intent = new KnowledgeRelationApplicationIntent(proposal, Now);
        Should.Throw<ArgumentNullException>(() => intent.RecordApplied(null!, Now));
        var receipt = new TagRelationMutationReceiptRecord(request.RequestId, request.RelationId, 1,
            request.Action, request.ApprovalDecisionId, Now);
        intent.RecordApplied(receipt, Now.AddMinutes(1));
        intent.State.ShouldBe(KnowledgeApplicationIntentState.Applied);
        intent.AppliedRelationVersion.ShouldBe(1);
        intent.RecordApplied(receipt, Now.AddMinutes(2));
        Should.Throw<Volo.Abp.BusinessException>(() => intent.MarkNeedsReview("late revoke", Now.AddMinutes(3)));
    }

    [Fact]
    public void Expired_Or_Revoked_Delayed_Apply_Moves_Pending_Intent_To_NeedsReview()
    {
        var request = KnowledgeRelationProposalTests.Request();
        var proposal = KnowledgeRelationProposalTests.Proposal(request, Guid.NewGuid());
        proposal.Approve(1, Guid.NewGuid(), Now, Now.AddHours(1), "Reviewed");
        var intent = new KnowledgeRelationApplicationIntent(proposal, Now);
        intent.MarkNeedsReview("Expired before Tags acknowledged the command.", Now.AddHours(2));
        intent.State.ShouldBe(KnowledgeApplicationIntentState.NeedsReview);
        intent.MarkNeedsReview("Retry", Now.AddHours(3));
        intent.State.ShouldBe(KnowledgeApplicationIntentState.NeedsReview);
    }
}
