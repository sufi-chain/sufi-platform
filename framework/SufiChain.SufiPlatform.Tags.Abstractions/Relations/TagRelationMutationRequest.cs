using System;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Tags.Relations;

public enum TagRelationMutationKind { Create = 0, Revise = 1, Retract = 2 }

/// <summary>Immutable proposed payload. Tenant and actor are resolved from trusted execution context.</summary>
public sealed class TagRelationMutationRequest
{
    public Guid RelationId { get; }
    public Guid DefinitionId { get; }
    public int ExpectedVersion { get; }
    public TagRelationMutationKind Action { get; }
    public TagEntityReference Scope { get; }
    public TagEntityReference Source { get; }
    public TagEntityReference Target { get; }
    public DateTime? ValidFromUtc { get; }
    public DateTime? ValidToUtc { get; }
    public Guid ApprovalDecisionId { get; }
    public Guid RequestId { get; }
    public string Reason { get; }

    public TagRelationMutationRequest(Guid relationId, Guid definitionId, int expectedVersion,
        TagRelationMutationKind action, TagEntityReference scope, TagEntityReference source, TagEntityReference target,
        DateTime? validFromUtc, DateTime? validToUtc, Guid approvalDecisionId, Guid requestId, string reason)
    {
        if (relationId == Guid.Empty || definitionId == Guid.Empty || approvalDecisionId == Guid.Empty || requestId == Guid.Empty)
            throw new ArgumentException("Relation, definition, approval and request identities are required.");
        if (!Enum.IsDefined(typeof(TagRelationMutationKind), action) ||
            (action == TagRelationMutationKind.Create ? expectedVersion != 0 : expectedVersion < 1))
            throw new ArgumentException("The action requires a valid expected version.");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 1024)
            throw new ArgumentException("A bounded reason is required.", nameof(reason));
        if ((validFromUtc.HasValue && validFromUtc.Value.Kind != DateTimeKind.Utc) ||
            (validToUtc.HasValue && validToUtc.Value.Kind != DateTimeKind.Utc) ||
            (validFromUtc.HasValue && validToUtc.HasValue && validFromUtc >= validToUtc))
            throw new ArgumentException("An ordered UTC effective interval is required.");
        RelationId = relationId;
        DefinitionId = definitionId;
        ExpectedVersion = expectedVersion;
        Action = action;
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        ValidFromUtc = validFromUtc;
        ValidToUtc = validToUtc;
        ApprovalDecisionId = approvalDecisionId;
        RequestId = requestId;
        Reason = reason.Trim();
    }
}

/// <summary>
/// The approval owner must load a durable decision and verify its exact payload,
/// version, actor, tenant, expiry and revocation state. An ID alone is never approval.
/// </summary>
public interface ITagRelationApprovalVerifier
{
    Task<bool> IsApprovedAsync(TagRelationMutationRequest request, Guid? tenantId, Guid actorId,
        CancellationToken cancellationToken = default);
}

public interface ITagRelationEndpointAccess
{
    Task<bool> CanReadAsync(TagEntityReference source, TagEntityReference target,
        CancellationToken cancellationToken = default);
    Task<bool> CanRelateInScopeAsync(TagEntityReference source, TagEntityReference target,
        TagEntityReference scope, CancellationToken cancellationToken = default);
}

public sealed class TagRelationMutationReceiptRecord
{
    public Guid RequestId { get; }
    public Guid RelationId { get; }
    public int AppliedVersion { get; }
    public TagRelationMutationKind Action { get; }
    public Guid ApprovalDecisionId { get; }
    public DateTime RecordedAtUtc { get; }

    public TagRelationMutationReceiptRecord(Guid requestId, Guid relationId, int appliedVersion,
        TagRelationMutationKind action, Guid approvalDecisionId, DateTime recordedAtUtc)
    {
        RequestId = requestId;
        RelationId = relationId;
        AppliedVersion = appliedVersion;
        Action = action;
        ApprovalDecisionId = approvalDecisionId;
        RecordedAtUtc = recordedAtUtc;
    }
}

/// <summary>
/// Tags-owned mutation boundary. Callers must not supply actor or approval flags.
/// Missing receipts are not treated as success.
/// </summary>
public interface ITagRelationMutationCoordinator
{
    Task<TagRelationMutationReceiptRecord> ApplyAsync(TagRelationMutationRequest request,
        CancellationToken cancellationToken = default);
    Task<TagRelationMutationReceiptRecord?> FindReceiptAsync(Guid requestId,
        CancellationToken cancellationToken = default);
}
