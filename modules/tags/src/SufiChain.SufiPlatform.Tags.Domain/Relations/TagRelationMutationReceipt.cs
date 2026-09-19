using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace SufiChain.SufiPlatform.Tags.Relations;

/// <summary>Durable Tags-side acknowledgement for one mutation request ID.</summary>
[DisableDateTimeNormalization]
public class TagRelationMutationReceipt : AggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string TenantScopeKey { get; private set; } = string.Empty;
    public Guid RelationId { get; private set; }
    public int AppliedVersion { get; private set; }
    public TagRelationMutationKind Action { get; private set; }
    public Guid ApprovalDecisionId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }

    protected TagRelationMutationReceipt() { }

    public TagRelationMutationReceipt(TagRelationMutationRequest request, Guid? tenantId, int appliedVersion,
        DateTime recordedAtUtc) : base(request?.RequestId ?? throw new ArgumentNullException(nameof(request)))
    {
        if (appliedVersion < 1) throw new ArgumentOutOfRangeException(nameof(appliedVersion));
        TenantId = tenantId;
        TenantScopeKey = tenantId?.ToString("N") ?? "host";
        RelationId = request.RelationId;
        AppliedVersion = appliedVersion;
        Action = request.Action;
        ApprovalDecisionId = request.ApprovalDecisionId;
        RecordedAtUtc = recordedAtUtc.Kind == DateTimeKind.Utc
            ? new DateTime(recordedAtUtc.Ticks - recordedAtUtc.Ticks % TimeSpan.TicksPerMillisecond, DateTimeKind.Utc)
            : throw new ArgumentException("UTC timestamps are required.");
    }

    public bool Matches(TagRelationMutationRequest request) =>
        request.RequestId == Id && request.RelationId == RelationId && request.Action == Action &&
        request.ApprovalDecisionId == ApprovalDecisionId;

    public TagRelationMutationReceiptRecord ToRecord() =>
        new(Id, RelationId, AppliedVersion, Action, ApprovalDecisionId, RecordedAtUtc);
}
