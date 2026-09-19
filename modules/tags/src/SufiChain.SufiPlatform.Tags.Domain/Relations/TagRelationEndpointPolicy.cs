using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tags.Relations;

/// <summary>
/// Fail-closed endpoint checks for future relation commands and queries.
/// This is not approval of a proposal or validation of a predicate/cardinality.
/// It must be rerun immediately before applying a delayed approved command.
/// </summary>
[ExposeServices(typeof(ITagRelationEndpointAccess), typeof(TagRelationEndpointPolicy))]
public class TagRelationEndpointPolicy : ITagRelationEndpointAccess, ITransientDependency
{
    private readonly IReadOnlyDictionary<string, ITagRelationEntityResolver> _resolvers;
    private readonly ICurrentTenant _currentTenant;

    public TagRelationEndpointPolicy(
        IEnumerable<ITagRelationEntityResolver> resolvers,
        ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
        var registry = new Dictionary<string, ITagRelationEntityResolver>(StringComparer.Ordinal);
        foreach (var resolver in resolvers)
        {
            if (!TagEntityReference.IsValidEntityType(resolver.EntityType) ||
                !registry.TryAdd(resolver.EntityType, resolver))
            {
                throw new InvalidOperationException("Relation entity resolvers require unique, valid type keys.");
            }
        }

        _resolvers = registry;
    }

    public virtual Task<bool> CanReadAsync(
        TagEntityReference source,
        TagEntityReference target,
        CancellationToken cancellationToken = default) =>
        CheckAsync(source, target, requireRelate: false, cancellationToken);

    public virtual Task<bool> CanRelateAsync(
        TagEntityReference source,
        TagEntityReference target,
        CancellationToken cancellationToken = default) =>
        CheckAsync(source, target, requireRelate: true, cancellationToken);

    public virtual Task<bool> CanRelateInScopeAsync(TagEntityReference source, TagEntityReference target,
        TagEntityReference scope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        return CheckAsync(source, target, requireRelate: true, cancellationToken, scope);
    }

    protected virtual async Task<bool> CheckAsync(
        TagEntityReference source,
        TagEntityReference target,
        bool requireRelate,
        CancellationToken cancellationToken,
        TagEntityReference? expectedScope = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        cancellationToken.ThrowIfCancellationRequested();
        var tenantId = _currentTenant.Id;
        var resolved = new Dictionary<TagEntityReference, TagRelationEntityResolution>();

        // Batch endpoints of the same type. Run adapters sequentially: they can
        // share a scoped DbContext and must not run concurrent queries on it.
        foreach (var group in new[] { source, target }.Distinct().GroupBy(x => x.EntityType))
        {
            if (!_resolvers.TryGetValue(group.Key, out var resolver))
            {
                return false;
            }

            var requested = group.ToList();
            var results = await resolver.ResolveAsync(requested, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (results == null || results.Count != requested.Count)
            {
                return false;
            }

            foreach (var result in results)
            {
                if (result == null || !requested.Contains(result.Reference) ||
                    result.TenantId != tenantId || !result.CanRead ||
                    (requireRelate && !result.CanRelate) || !resolved.TryAdd(result.Reference, result))
                {
                    return false;
                }
            }
        }

        return _currentTenant.Id == tenantId &&
            resolved[source].Scope.Equals(resolved[target].Scope) &&
            (expectedScope == null || resolved[source].Scope.Equals(expectedScope));
    }
}
