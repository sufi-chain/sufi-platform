using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tags.Relations;

/// <summary>
/// One immutable vocabulary revision. A changed meaning requires a new ID and
/// revision; accepted relations must retain their original definition ID.
/// This validates shape only, not endpoint access, cycles, or approval.
/// </summary>
public class TagRelationDefinition : AggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    // A non-null namespace makes host uniqueness portable across providers.
    public string TenantScopeKey { get; protected set; } = string.Empty;
    public Guid PredicateTagId { get; protected set; }
    public string StableKey { get; protected set; } = string.Empty;
    public int Revision { get; protected set; }
    public string SourceEntityType { get; protected set; } = string.Empty;
    public string TargetEntityType { get; protected set; } = string.Empty;
    public bool IsSymmetric { get; protected set; }
    public bool AllowsSelfReference { get; protected set; }
    public bool RequiresAcyclicGraph { get; protected set; }

    protected TagRelationDefinition() { }

    public TagRelationDefinition(Guid id, Guid? tenantId, Guid predicateTagId,
        string stableKey, int revision, string sourceEntityType, string targetEntityType,
        bool isSymmetric = false, bool allowsSelfReference = false, bool requiresAcyclicGraph = false)
        : base(id)
    {
        if (id == Guid.Empty || predicateTagId == Guid.Empty || tenantId == Guid.Empty)
            throw new ArgumentException("Non-empty identity values are required.");
        if (revision < 1)
            throw new ArgumentOutOfRangeException(nameof(revision));
        if (!TagEntityReference.IsValidEntityType(stableKey) ||
            !TagEntityReference.IsValidEntityType(sourceEntityType) ||
            !TagEntityReference.IsValidEntityType(targetEntityType))
            throw new ArgumentException("Stable lowercase vocabulary and endpoint keys are required.");
        if (isSymmetric && sourceEntityType != targetEntityType)
            throw new ArgumentException("Symmetric definitions require identical endpoint types.");
        if (requiresAcyclicGraph && (isSymmetric || allowsSelfReference || sourceEntityType != targetEntityType))
            throw new ArgumentException("Cycle-guarded definitions require directed, same-type, non-self endpoints.");

        TenantId = tenantId;
        TenantScopeKey = tenantId?.ToString("N") ?? "host";
        PredicateTagId = predicateTagId;
        StableKey = stableKey;
        Revision = revision;
        SourceEntityType = sourceEntityType;
        TargetEntityType = targetEntityType;
        IsSymmetric = isSymmetric;
        AllowsSelfReference = allowsSelfReference;
        RequiresAcyclicGraph = requiresAcyclicGraph;
    }

    public virtual bool AcceptsEndpoints(TagEntityReference source, TagEntityReference target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        return source.EntityType == SourceEntityType && target.EntityType == TargetEntityType &&
            (AllowsSelfReference || !source.Equals(target));
    }

    /// <summary>Canonical order for symmetric duplicate detection; directed order is preserved.</summary>
    public virtual (TagEntityReference Source, TagEntityReference Target) Canonicalize(
        TagEntityReference source, TagEntityReference target)
    {
        if (!AcceptsEndpoints(source, target))
            throw new ArgumentException("Endpoints do not satisfy this definition.");
        return IsSymmetric && source.EntityId.CompareTo(target.EntityId) > 0
            ? (target, source) : (source, target);
    }
}
