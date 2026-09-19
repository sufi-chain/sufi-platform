using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareImprovementProposalTests
{
    [Fact]
    public void Should_Require_Review_Before_Apply()
    {
        var proposal = CreateProposal();

        Should.Throw<InvalidOperationException>(() =>
            proposal.MarkApplied(Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void Should_Record_Review_And_Apply_Audit_Data()
    {
        var proposal = CreateProposal();
        var reviewerId = Guid.NewGuid();
        var applierId = Guid.NewGuid();
        var reviewedTime = new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc);
        var appliedTime = reviewedTime.AddMinutes(5);

        proposal.MarkReviewed(reviewerId, reviewedTime);
        proposal.MarkApplied(applierId, appliedTime);

        proposal.Status.ShouldBe(HooshvareImprovementProposalStatus.Applied);
        proposal.ReviewedBy.ShouldBe(reviewerId);
        proposal.ReviewedTime.ShouldBe(reviewedTime);
        proposal.AppliedBy.ShouldBe(applierId);
        proposal.AppliedTime.ShouldBe(appliedTime);
    }

    [Fact]
    public void Should_Reject_Pending_Proposal()
    {
        var proposal = CreateProposal();
        var reviewerId = Guid.NewGuid();
        var reviewedTime = new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc);

        proposal.Reject(reviewerId, reviewedTime);

        proposal.Status.ShouldBe(HooshvareImprovementProposalStatus.Rejected);
        proposal.ReviewedBy.ShouldBe(reviewerId);
        proposal.ReviewedTime.ShouldBe(reviewedTime);
    }

    private static HooshvareImprovementProposal CreateProposal()
    {
        return new HooshvareImprovementProposal(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "base-fingerprint",
            """{"systemPrompt":"Improved"}""",
            "Improves clarity.");
    }
}
