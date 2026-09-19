using SufiChain.SufiPlatform.Tags.Features;
using SufiChain.SufiPlatform.Tags.Permissions;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Tags.Relations;

/// <summary>
/// Internal preflight for a future transactional command coordinator. Does not
/// persist a mutation, reserve a request ID, or replace write-time graph locking.
/// There is intentionally no default approval verifier and no remote API.
/// </summary>
public class TagRelationCommandPolicy : ITransientDependency
{
    private readonly ICurrentTenant _tenant;
    private readonly ICurrentUser _user;
    private readonly IPermissionChecker _permissions;
    private readonly IFeatureChecker _features;
    private readonly TagRelationEndpointPolicy _endpoints;
    private readonly IRepository<TagRelationDefinition, Guid> _definitions;
    private readonly ITagRepository _tags;
    private readonly IEntityRelationRepository _relations;
    private readonly IReadOnlyList<ITagRelationApprovalVerifier> _verifiers;

    public TagRelationCommandPolicy(ICurrentTenant tenant, ICurrentUser user, IPermissionChecker permissions,
        IFeatureChecker features, TagRelationEndpointPolicy endpoints, IRepository<TagRelationDefinition, Guid> definitions,
        ITagRepository tags, IEntityRelationRepository relations, IEnumerable<ITagRelationApprovalVerifier> verifiers)
    {
        _tenant = tenant;
        _user = user;
        _permissions = permissions;
        _features = features;
        _endpoints = endpoints;
        _definitions = definitions;
        _tags = tags;
        _relations = relations;
        _verifiers = verifiers.ToArray();
    }

    public virtual async Task ValidateAsync(TagRelationMutationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var tenantId = _tenant.Id;
        var actorId = _user.Id;
        if (!_user.IsAuthenticated || !actorId.HasValue || actorId == Guid.Empty || _verifiers.Count != 1 ||
            !await _features.IsEnabledAsync(SufiTagsFeatures.Enable) || !await _features.IsEnabledAsync(SufiTagsFeatures.Relations) ||
            !await _permissions.IsGrantedAsync(TagsPermissions.Relations.Default) ||
            !await _permissions.IsGrantedAsync(TagsPermissions.Relations.Mutate))
            throw Denied();

        var definition = await _definitions.FindAsync(request.DefinitionId, cancellationToken: cancellationToken);
        if (definition == null || definition.TenantId != tenantId || !definition.AcceptsEndpoints(request.Source, request.Target))
            throw Denied();
        // No safe graph-wide locking strategy has been installed yet. Do not accept
        // an acyclic assertion merely because its individual endpoints are valid.
        if (definition.RequiresAcyclicGraph && request.Action != TagRelationMutationKind.Retract)
            throw Denied();
        var predicate = await _tags.FindAsync(definition.PredicateTagId, cancellationToken: cancellationToken);
        if (predicate == null || predicate.IsDeleted || predicate.TenantId != tenantId ||
            predicate.Kind != TagKind.RelationPredicate || predicate.StableKey != definition.StableKey)
            throw Denied();

        if (request.Action != TagRelationMutationKind.Create)
        {
            var relation = await _relations.FindAsync(request.RelationId, cancellationToken: cancellationToken);
            var (source, target) = definition.Canonicalize(request.Source, request.Target);
            if (relation == null || relation.TenantId != tenantId || relation.DefinitionId != definition.Id ||
                relation.ScopeType != request.Scope.EntityType || relation.ScopeId != request.Scope.EntityId ||
                relation.SourceType != source.EntityType || relation.SourceId != source.EntityId ||
                relation.TargetType != target.EntityType || relation.TargetId != target.EntityId)
                throw Denied();
        }
        if (!await _endpoints.CanRelateInScopeAsync(request.Source, request.Target, request.Scope, cancellationToken) ||
            !await _verifiers[0].IsApprovedAsync(request, tenantId, actorId.Value, cancellationToken))
            throw Denied();
        cancellationToken.ThrowIfCancellationRequested();
        if (_tenant.Id != tenantId || !_user.IsAuthenticated || _user.Id != actorId)
            throw Denied();
    }

    private static BusinessException Denied() => new(TagsErrorCodes.RelationCommandDenied);
}
