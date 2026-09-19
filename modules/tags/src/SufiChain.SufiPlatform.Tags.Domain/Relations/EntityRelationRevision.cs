namespace SufiChain.SufiPlatform.Tags.Relations;

/// <summary>An immutable snapshot. Null effective bounds mean unknown, not infinity.</summary>
public sealed class EntityRelationRevision
{
    public int Sequence { get; private set; }
    public Guid DefinitionId { get; private set; }
    public int DefinitionRevision { get; private set; }
    public RelationRevisionAction Action { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public DateTime? ValidFromUtc { get; private set; }
    public DateTime? ValidToUtc { get; private set; }
    public Guid ActorId { get; private set; }
    public Guid ApprovalDecisionId { get; private set; }
    public Guid RequestId { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private EntityRelationRevision() { }

    internal EntityRelationRevision(int sequence, Guid definitionId, int definitionRevision,
        RelationRevisionAction action, DateTime recordedAtUtc, DateTime? validFromUtc, DateTime? validToUtc,
        Guid actorId, Guid approvalDecisionId, Guid requestId, string reason)
    {
        if (actorId == Guid.Empty || approvalDecisionId == Guid.Empty || requestId == Guid.Empty)
            throw new ArgumentException("Actor, approval and request identities are required.");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > EntityRelation.MaxReasonLength)
            throw new ArgumentException("A bounded change reason is required.", nameof(reason));
        if (recordedAtUtc.Kind != DateTimeKind.Utc ||
            (validFromUtc.HasValue && validFromUtc.Value.Kind != DateTimeKind.Utc) ||
            (validToUtc.HasValue && validToUtc.Value.Kind != DateTimeKind.Utc))
            throw new ArgumentException("Relation timestamps must be UTC.");
        // BSON dates retain milliseconds; use the same precision in both providers
        // so a retry remains identical after a persistence round trip.
        recordedAtUtc = ToMilliseconds(recordedAtUtc);
        validFromUtc = validFromUtc.HasValue ? ToMilliseconds(validFromUtc.Value) : null;
        validToUtc = validToUtc.HasValue ? ToMilliseconds(validToUtc.Value) : null;
        if (validFromUtc.HasValue && validToUtc.HasValue && validFromUtc >= validToUtc)
            throw new ArgumentException("The effective interval must have a positive duration.");

        Sequence = sequence;
        DefinitionId = definitionId;
        DefinitionRevision = definitionRevision;
        Action = action;
        RecordedAtUtc = recordedAtUtc;
        ValidFromUtc = validFromUtc;
        ValidToUtc = validToUtc;
        ActorId = actorId;
        ApprovalDecisionId = approvalDecisionId;
        RequestId = requestId;
        Reason = reason.Trim();
    }

    internal bool HasSamePayload(EntityRelationRevision other) => Action == other.Action &&
        DefinitionId == other.DefinitionId && DefinitionRevision == other.DefinitionRevision &&
        ValidFromUtc == other.ValidFromUtc && ValidToUtc == other.ValidToUtc && ActorId == other.ActorId &&
        ApprovalDecisionId == other.ApprovalDecisionId && Reason == other.Reason;

    private static DateTime ToMilliseconds(DateTime value) =>
        new(value.Ticks - value.Ticks % TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
}
