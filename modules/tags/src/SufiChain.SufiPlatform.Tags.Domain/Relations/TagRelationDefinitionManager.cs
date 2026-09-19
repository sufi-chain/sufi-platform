using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiPlatform.Tags.Relations;

/// <summary>Trusted domain entry point; application authorization is required separately.</summary>
public class TagRelationDefinitionManager : DomainService
{
    private readonly ITagRepository _tags;
    private readonly IRepository<TagRelationDefinition, Guid> _definitions;

    public TagRelationDefinitionManager(ITagRepository tags, IRepository<TagRelationDefinition, Guid> definitions)
    {
        _tags = tags;
        _definitions = definitions;
    }

    public virtual async Task<TagRelationDefinition> CreateRevisionAsync(Guid predicateTagId, int expectedRevision,
        string sourceType, string targetType, bool symmetric = false, bool allowsSelfReference = false,
        bool requiresAcyclicGraph = false, CancellationToken cancellationToken = default)
    {
        if (expectedRevision < 0 || expectedRevision == int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(expectedRevision));
        var tenantId = CurrentTenant.Id;
        var predicate = await _tags.FindAsync(predicateTagId, cancellationToken: cancellationToken);
        if (predicate == null || predicate.IsDeleted || predicate.TenantId != tenantId ||
            predicate.Kind != TagKind.RelationPredicate || !TagEntityReference.IsValidEntityType(predicate.StableKey))
            throw new BusinessException(TagsErrorCodes.InvalidRelationDefinition);

        var revisions = await _definitions.GetListAsync(x => x.TenantId == tenantId &&
            (x.PredicateTagId == predicateTagId || x.StableKey == predicate.StableKey),
            includeDetails: false, cancellationToken: cancellationToken);
        if (CurrentTenant.Id != tenantId || revisions.Any(x => x.TenantId != tenantId ||
            x.PredicateTagId != predicateTagId || x.StableKey != predicate.StableKey))
            throw new BusinessException(TagsErrorCodes.InvalidRelationDefinition);
        if (revisions.Select(x => x.Revision).DefaultIfEmpty(0).Max() != expectedRevision)
            throw new BusinessException(TagsErrorCodes.RelationVersionConflict);

        // Database unique indexes arbitrate competing attempts at the same revision.
        var definition = new TagRelationDefinition(GuidGenerator.Create(), tenantId, predicateTagId,
            predicate.StableKey!, checked(expectedRevision + 1), sourceType, targetType,
            symmetric, allowsSelfReference, requiresAcyclicGraph);
        return await _definitions.InsertAsync(definition, autoSave: true, cancellationToken: cancellationToken);
    }
}
