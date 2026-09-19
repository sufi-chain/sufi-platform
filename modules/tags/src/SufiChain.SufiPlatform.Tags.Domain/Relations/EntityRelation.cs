using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tags.Relations;

/// <summary>
/// Accepted-relation state machine. The owning command workflow must verify approval,
/// current endpoint access and graph constraints before calling this domain API.
/// Endpoints and predicate identity are fixed; replacement requires a new relation.
/// </summary>
public class EntityRelation : AggregateRoot<Guid>, IMultiTenant
{
    public const int MaxReasonLength = 1024;
    public const int MaxRevisions = 256;
    private List<EntityRelationRevision> _revisions = new();

    public Guid? TenantId { get; protected set; }
    public string TenantScopeKey { get; protected set; } = string.Empty;
    public string ScopeType { get; protected set; } = string.Empty;
    public Guid ScopeId { get; protected set; }
    public Guid PredicateTagId { get; protected set; }
    public Guid DefinitionId { get; protected set; }
    public int DefinitionRevision { get; protected set; }
    public string SourceType { get; protected set; } = string.Empty;
    public Guid SourceId { get; protected set; }
    public string TargetType { get; protected set; } = string.Empty;
    public Guid TargetId { get; protected set; }
    public int Version { get; protected set; }
    public bool IsRetracted { get; protected set; }
    public IReadOnlyCollection<EntityRelationRevision> Revisions => _revisions.OrderBy(x => x.Sequence).ToList().AsReadOnly();

    protected EntityRelation() { }

    public EntityRelation(Guid id, Guid? tenantId, TagRelationDefinition definition,
        TagEntityReference scope, TagEntityReference source, TagEntityReference target,
        DateTime recordedAtUtc, DateTime? validFromUtc, DateTime? validToUtc,
        Guid actorId, Guid approvalDecisionId, Guid requestId, string reason) : base(id)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(scope);
        if (id == Guid.Empty || tenantId == Guid.Empty || definition.TenantId != tenantId)
            throw new ArgumentException("The relation must belong to its definition's tenant.");
        (source, target) = definition.Canonicalize(source, target);
        TenantId = tenantId;
        TenantScopeKey = tenantId?.ToString("N") ?? "host";
        ScopeType = scope.EntityType;
        ScopeId = scope.EntityId;
        PredicateTagId = definition.PredicateTagId;
        DefinitionId = definition.Id;
        DefinitionRevision = definition.Revision;
        SourceType = source.EntityType;
        SourceId = source.EntityId;
        TargetType = target.EntityType;
        TargetId = target.EntityId;
        _revisions.Add(new EntityRelationRevision(1, DefinitionId, DefinitionRevision,
            RelationRevisionAction.Accept, recordedAtUtc, validFromUtc, validToUtc,
            actorId, approvalDecisionId, requestId, reason));
        Version = 1;
    }

    public virtual int Revise(int expectedVersion, DateTime recordedAtUtc, DateTime? validFromUtc,
        DateTime? validToUtc, Guid actorId, Guid approvalDecisionId, Guid requestId, string reason) =>
        Append(expectedVersion, RelationRevisionAction.Revise, recordedAtUtc, validFromUtc, validToUtc,
            actorId, approvalDecisionId, requestId, reason);

    public virtual int Retract(int expectedVersion, DateTime recordedAtUtc, Guid actorId,
        Guid approvalDecisionId, Guid requestId, string reason)
    {
        var current = _revisions.Single(x => x.Sequence == Version);
        return Append(expectedVersion, RelationRevisionAction.Retract, recordedAtUtc,
            current.ValidFromUtc, current.ValidToUtc, actorId, approvalDecisionId, requestId, reason);
    }

    private int Append(int expectedVersion, RelationRevisionAction action, DateTime recordedAtUtc,
        DateTime? validFromUtc, DateTime? validToUtc, Guid actorId, Guid approvalDecisionId, Guid requestId, string reason)
    {
        var candidate = new EntityRelationRevision(checked(Version + 1), DefinitionId, DefinitionRevision,
            action, recordedAtUtc, validFromUtc, validToUtc, actorId, approvalDecisionId, requestId, reason);
        var previousRequest = _revisions.SingleOrDefault(x => x.RequestId == requestId);
        if (previousRequest != null)
        {
            if (!previousRequest.HasSamePayload(candidate) || expectedVersion != previousRequest.Sequence - 1)
                throw new BusinessException(TagsErrorCodes.RelationRequestConflict);
            return previousRequest.Sequence;
        }
        if (expectedVersion != Version)
            throw new BusinessException(TagsErrorCodes.RelationVersionConflict);
        if (IsRetracted)
            throw new BusinessException(TagsErrorCodes.RelationRetracted);
        // Reserve the last slot for withdrawal even when revision capacity is exhausted.
        if (_revisions.Count >= MaxRevisions ||
            (action != RelationRevisionAction.Retract && _revisions.Count >= MaxRevisions - 1))
            throw new BusinessException(TagsErrorCodes.RelationHistoryLimit);
        if (candidate.RecordedAtUtc < _revisions.Single(x => x.Sequence == Version).RecordedAtUtc)
            throw new ArgumentException("Recorded time cannot move backwards.", nameof(recordedAtUtc));
        _revisions.Add(candidate);
        Version = candidate.Sequence;
        IsRetracted = action == RelationRevisionAction.Retract;
        return Version;
    }
}
