using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace SufiChain.SufiPlatform.SufiAI.Knowledge;

/// <summary>
/// One immutable proposal version and its reserved decision identity. This trusted
/// domain API does not establish caller authorization or expose human review to tools.
/// Corrections require another version; approval is not relationship application.
/// </summary>
[DisableDateTimeNormalization]
public class KnowledgeRelationProposal : AggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string TenantScopeKey { get; private set; } = null!;
    public Guid ProposalId { get; private set; }
    public int ProposalVersion { get; private set; }
    public int Version { get; private set; }
    public KnowledgeProposalState State { get; private set; }
    public Guid ProposedBy { get; private set; }
    public DateTime ProposedAtUtc { get; private set; }
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
    // Ticks retain exact proposed bounds across providers (MongoDB dates have ms precision).
    public long? ValidFromUtcTicks { get; private set; }
    public long? ValidToUtcTicks { get; private set; }
    public Guid RequestId { get; private set; }
    public string Reason { get; private set; } = null!;
    public string EvidenceType { get; private set; } = null!;
    public Guid EvidenceId { get; private set; }
    public string EvidenceVersion { get; private set; } = null!;
    public string EvidenceSha256 { get; private set; } = null!;
    public string EvidenceLocator { get; private set; } = null!;
    public Guid? ReviewerId { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public string? ReviewReason { get; private set; }
    public Guid? RevokedBy { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevocationReason { get; private set; }

    protected KnowledgeRelationProposal() { }

    public KnowledgeRelationProposal(Guid proposalId, int proposalVersion, Guid? tenantId,
        TagRelationMutationRequest request, TagEntityReference evidence, string evidenceVersion,
        string evidenceSha256, string evidenceLocator, Guid proposedBy, DateTime proposedAtUtc)
        : base(request?.ApprovalDecisionId ?? throw new ArgumentNullException(nameof(request)))
    {
        if (proposalId == Guid.Empty || proposedBy == Guid.Empty || proposalVersion < 1 || tenantId == Guid.Empty)
            throw new ArgumentException("Valid proposal, author, version and tenant identities are required.");
        ArgumentNullException.ThrowIfNull(evidence);
        if (evidenceSha256 == null || evidenceSha256.Length != 64 ||
            evidenceSha256.Any(c => !(c is >= '0' and <= '9' or >= 'a' and <= 'f')))
            throw new ArgumentException("A lowercase SHA-256 evidence digest is required.", nameof(evidenceSha256));
        TenantId = tenantId;
        TenantScopeKey = tenantId?.ToString("N") ?? "host";
        ProposalId = proposalId;
        ProposalVersion = proposalVersion;
        Version = 1;
        ProposedBy = proposedBy;
        ProposedAtUtc = UtcMilliseconds(proposedAtUtc);
        RelationId = request.RelationId;
        DefinitionId = request.DefinitionId;
        ExpectedRelationVersion = request.ExpectedVersion;
        Action = request.Action;
        ScopeType = request.Scope.EntityType;
        ScopeId = request.Scope.EntityId;
        SourceType = request.Source.EntityType;
        SourceId = request.Source.EntityId;
        TargetType = request.Target.EntityType;
        TargetId = request.Target.EntityId;
        ValidFromUtcTicks = request.ValidFromUtc?.Ticks;
        ValidToUtcTicks = request.ValidToUtc?.Ticks;
        RequestId = request.RequestId;
        Reason = request.Reason;
        EvidenceType = evidence.EntityType;
        EvidenceId = evidence.EntityId;
        EvidenceVersion = Bounded(evidenceVersion, 128);
        EvidenceSha256 = evidenceSha256;
        EvidenceLocator = Bounded(evidenceLocator, 512);
    }

    public void Approve(int expectedVersion, Guid reviewerId, DateTime reviewedAtUtc,
        DateTime expiresAtUtc, string reason)
    {
        EnsurePending(expectedVersion, reviewerId);
        var reviewed = UtcMilliseconds(reviewedAtUtc);
        var expires = UtcMilliseconds(expiresAtUtc);
        var rationale = Bounded(reason, 1024);
        if (reviewed < ProposedAtUtc || expires <= reviewed || expires - reviewed > TimeSpan.FromHours(24))
            throw new BusinessException(AIErrorCodes.KnowledgeReviewInvalid);
        ReviewerId = reviewerId;
        ReviewedAtUtc = reviewed;
        ExpiresAtUtc = expires;
        ReviewReason = rationale;
        State = KnowledgeProposalState.Approved;
        Version++;
    }

    public void Reject(int expectedVersion, Guid reviewerId, DateTime reviewedAtUtc, string reason)
    {
        EnsurePending(expectedVersion, reviewerId);
        var reviewed = UtcMilliseconds(reviewedAtUtc);
        var rationale = Bounded(reason, 1024);
        if (reviewed < ProposedAtUtc) throw new BusinessException(AIErrorCodes.KnowledgeReviewInvalid);
        ReviewerId = reviewerId;
        ReviewedAtUtc = reviewed;
        ReviewReason = rationale;
        State = KnowledgeProposalState.Rejected;
        Version++;
    }

    public void Revoke(int expectedVersion, Guid actorId, DateTime revokedAtUtc, string reason)
    {
        var revoked = UtcMilliseconds(revokedAtUtc);
        var rationale = Bounded(reason, 1024);
        if (Version != expectedVersion || State != KnowledgeProposalState.Approved ||
            actorId == Guid.Empty || revoked < ReviewedAtUtc)
            throw new BusinessException(AIErrorCodes.KnowledgeReviewInvalid);
        RevokedBy = actorId;
        RevokedAtUtc = revoked;
        RevocationReason = rationale;
        State = KnowledgeProposalState.Revoked;
        Version++;
    }

    public TagRelationMutationRequest ToMutationRequest() => new(RelationId, DefinitionId, ExpectedRelationVersion,
        Action, new TagEntityReference(ScopeType, ScopeId), new TagEntityReference(SourceType, SourceId),
        new TagEntityReference(TargetType, TargetId),
        ValidFromUtcTicks.HasValue ? new DateTime(ValidFromUtcTicks.Value, DateTimeKind.Utc) : null,
        ValidToUtcTicks.HasValue ? new DateTime(ValidToUtcTicks.Value, DateTimeKind.Utc) : null,
        Id, RequestId, Reason);

    public KnowledgeProposalEvidence ToEvidence() => new(TenantId, ScopeType, ScopeId, EvidenceType, EvidenceId,
        EvidenceVersion, EvidenceSha256, EvidenceLocator);

    public bool Authorizes(TagRelationMutationRequest request, Guid? tenantId, Guid actorId, DateTime nowUtc)
    {
        if (nowUtc.Kind != DateTimeKind.Utc) return false;
        return State == KnowledgeProposalState.Approved && TenantId == tenantId && actorId != Guid.Empty &&
            ReviewerId == actorId && ReviewedAtUtc <= nowUtc && ExpiresAtUtc > nowUtc &&
            Id == request.ApprovalDecisionId && RelationId == request.RelationId && DefinitionId == request.DefinitionId &&
            ExpectedRelationVersion == request.ExpectedVersion && Action == request.Action &&
            ScopeType == request.Scope.EntityType && ScopeId == request.Scope.EntityId &&
            SourceType == request.Source.EntityType && SourceId == request.Source.EntityId &&
            TargetType == request.Target.EntityType && TargetId == request.Target.EntityId &&
            ValidFromUtcTicks == request.ValidFromUtc?.Ticks && ValidToUtcTicks == request.ValidToUtc?.Ticks &&
            RequestId == request.RequestId && Reason == request.Reason;
    }

    private void EnsurePending(int expectedVersion, Guid reviewerId)
    {
        if (Version != expectedVersion || State != KnowledgeProposalState.Pending || reviewerId == Guid.Empty)
            throw new BusinessException(AIErrorCodes.KnowledgeReviewInvalid);
    }

    private static string Bounded(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > maxLength)
            throw new ArgumentException("A bounded nonempty value is required.");
        return value.Trim();
    }

    private static DateTime UtcMilliseconds(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc) throw new ArgumentException("UTC timestamps are required.");
        return new DateTime(value.Ticks - value.Ticks % TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
    }
}
