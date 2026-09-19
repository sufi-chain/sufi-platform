using Volo.Abp;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tags.Tags;

public class Tag : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string NormalizedName { get; protected set; } = string.Empty;
    public string Scope { get; protected set; } = string.Empty;
    public string? Color { get; protected set; }
    public TagKind Kind { get; protected set; } = TagKind.Classification;
    public string? StableKey { get; protected set; }

    protected Tag()
    {
    }

    public Tag(Guid id, string name, string scope, Guid? tenantId = null, string? color = null) : base(id)
    {
        TenantId = tenantId;
        SetName(name);
        SetScope(scope);
        SetColor(color);
    }

    public static Tag CreateRelationPredicate(Guid id, string name, string scope, string stableKey, Guid? tenantId = null)
    {
        if (id == Guid.Empty || tenantId == Guid.Empty || !TagEntityReference.IsValidEntityType(stableKey))
            throw new ArgumentException("A canonical predicate key and valid identities are required.");
        return new Tag(id, name, scope, tenantId) { Kind = TagKind.RelationPredicate, StableKey = stableKey };
    }

    public virtual void EnsureClassification()
    {
        if (Kind != TagKind.Classification)
            throw new BusinessException(TagsErrorCodes.PredicateRequiresRelationWorkflow);
    }

    public virtual void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), TagConsts.MaxNameLength);
        NormalizedName = Name.Trim().ToUpperInvariant();
    }

    public virtual void SetScope(string scope)
    {
        Scope = Check.NotNullOrWhiteSpace(scope, nameof(scope), TagScopeConsts.MaxScopeLength);
    }

    public virtual void SetColor(string? color)
    {
        if (!string.IsNullOrWhiteSpace(color) && color.Length > TagConsts.MaxColorLength)
        {
            throw new BusinessException(TagsErrorCodes.TagAlreadyExists)
                .WithData("MaxColorLength", TagConsts.MaxColorLength);
        }

        Color = color;
    }
}
