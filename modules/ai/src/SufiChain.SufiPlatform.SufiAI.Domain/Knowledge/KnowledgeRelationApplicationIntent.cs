using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace SufiChain.SufiPlatform.SufiAI.Knowledge;

/// <summary>
/// Durable apply command. Identity is the mutation request ID so retries stay stable.
/// Approval is not application success; Applied requires a Tags receipt.
/// </summary>
[DisableDateTimeNormalization]
public class KnowledgeRelationApplicationIntent : AggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string TenantScopeKey { get; private set; } = null!;
    public Guid ProposalId { get; private set; }
    public int ProposalVersion { get; private set; }
    public Guid ApprovalDecisionId { get; private set; }
    public Guid ReviewerId { get; private set; }
    public Guid RelationId { get; private set; }
    public Guid DefinitionId { get; private set; }
    public int ExpectedRelationVersion { get; private set; }
    public TagRelationMutationKind Action { get; private set; }
    public string ScopeType { get; private set; } = null!;
    public Guid ScopeId { get; private set; }
    public string SourceType { get; private set; } = null!;
    public Guid SourceId { get; private set; }
    public string TargetType { get; private set; } = null!;
    public Guid TargetId { get; private set; }
    public long? ValidFromUtcTicks { get; private set; }
    public long? ValidToUtcTicks { get; private set; }
    public string Reason { get; private set; } = null!;
    public KnowledgeApplicationIntentState State { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? AppliedAtUtc { get; private set; }
    public int? AppliedRelationVersion { get; private set; }
    public DateTime? NeedsReviewAtUtc { get; private set; }
    public string? FailureReason { get; private set; }

    protected KnowledgeRelationApplicationIntent() { }

    public KnowledgeRelationApplicationIntent(KnowledgeRelationProposal proposal, DateTime createdAtUtc)
        : base(proposal?.RequestId ?? throw new ArgumentNullException(nameof(proposal)))
    {
        if (proposal.State != KnowledgeProposalState.Approved || !proposal.ReviewerId.HasValue)
            throw new BusinessException(AIErrorCodes.KnowledgeReviewInvalid);
        TenantId = proposal.TenantId;
        TenantScopeKey = proposal.TenantScopeKey;
        ProposalId = proposal.ProposalId;
        ProposalVersion = proposal.ProposalVersion;
        ApprovalDecisionId = proposal.Id;
        ReviewerId = proposal.ReviewerId.Value;
        RelationId = proposal.RelationId;
        DefinitionId = proposal.DefinitionId;
        ExpectedRelationVersion = proposal.ExpectedRelationVersion;
        Action = proposal.Action;
        ScopeType = proposal.ScopeType;
        ScopeId = proposal.ScopeId;
        SourceType = proposal.SourceType;
        SourceId = proposal.SourceId;
        TargetType = proposal.TargetType;
        TargetId = proposal.TargetId;
        ValidFromUtcTicks = proposal.ValidFromUtcTicks;
        ValidToUtcTicks = proposal.ValidToUtcTicks;
        Reason = proposal.Reason;
        CreatedAtUtc = UtcMilliseconds(createdAtUtc);
        State = KnowledgeApplicationIntentState.PendingApply;
    }

    public TagRelationMutationRequest ToMutationRequest() => new(RelationId, DefinitionId, ExpectedRelationVersion,
        Action, new TagEntityReference(ScopeType, ScopeId), new TagEntityReference(SourceType, SourceId),
        new TagEntityReference(TargetType, TargetId),
        ValidFromUtcTicks.HasValue ? new DateTime(ValidFromUtcTicks.Value, DateTimeKind.Utc) : null,
        ValidToUtcTicks.HasValue ? new DateTime(ValidToUtcTicks.Value, DateTimeKind.Utc) : null,
        ApprovalDecisionId, Id, Reason);

    public void RecordApplied(TagRelationMutationReceiptRecord receipt, DateTime appliedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        if (receipt.RequestId != Id || receipt.RelationId != RelationId || receipt.Action != Action ||
            receipt.ApprovalDecisionId != ApprovalDecisionId)
            throw new BusinessException(AIErrorCodes.KnowledgeReviewInvalid);
        if (State == KnowledgeApplicationIntentState.Applied)
        {
            if (AppliedRelationVersion != receipt.AppliedVersion)
                throw new BusinessException(AIErrorCodes.KnowledgeReviewInvalid);
            return;
        }
        if (State != KnowledgeApplicationIntentState.PendingApply)
            throw new BusinessException(AIErrorCodes.KnowledgeReviewInvalid);
        AppliedRelationVersion = receipt.AppliedVersion;
        AppliedAtUtc = UtcMilliseconds(appliedAtUtc);
        State = KnowledgeApplicationIntentState.Applied;
    }

    public void MarkNeedsReview(string reason, DateTime atUtc)
    {
        if (State == KnowledgeApplicationIntentState.Applied)
            throw new BusinessException(AIErrorCodes.KnowledgeReviewInvalid);
        if (State == KnowledgeApplicationIntentState.NeedsReview) return;
        FailureReason = string.IsNullOrWhiteSpace(reason) ? "NeedsReview" : reason.Trim();
        if (FailureReason.Length > 1024) FailureReason = FailureReason.Substring(0, 1024);
        NeedsReviewAtUtc = UtcMilliseconds(atUtc);
        State = KnowledgeApplicationIntentState.NeedsReview;
    }

    private static DateTime UtcMilliseconds(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc) throw new ArgumentException("UTC timestamps are required.");
        return new DateTime(value.Ticks - value.Ticks % TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
    }
}
