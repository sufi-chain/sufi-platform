using System.Security.Cryptography;
using System.Text;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tags.Relations;

[ExposeServices(typeof(IDataSeedContributor), typeof(TagRelationVocabularyDataSeedContributor))]
public class TagRelationVocabularyDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IEnumerable<ITagRelationVocabularyContributor> _contributors;
    private readonly ITagRepository _tags;
    private readonly IRepository<TagRelationDefinition, Guid> _definitions;
    private readonly TagRelationDefinitionManager _manager;
    private readonly ICurrentTenant _tenant;
    private readonly IDataFilter _filters;

    public TagRelationVocabularyDataSeedContributor(IEnumerable<ITagRelationVocabularyContributor> contributors,
        ITagRepository tags, IRepository<TagRelationDefinition, Guid> definitions,
        TagRelationDefinitionManager manager, ICurrentTenant tenant, IDataFilter filters)
    {
        _contributors = contributors;
        _tags = tags;
        _definitions = definitions;
        _manager = manager;
        _tenant = tenant;
        _filters = filters;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        var entries = _contributors.SelectMany(contributor => contributor.GetEntries()
            .Select(entry => (contributor.Scope, Entry: entry))).ToArray();
        if (entries.Any(x => !TagEntityReference.IsValidEntityType(x.Scope)) ||
            entries.GroupBy(x => x.Entry.StableKey, StringComparer.Ordinal).Any(x => x.Count() != 1))
            throw new BusinessException(TagsErrorCodes.InvalidRelationDefinition);

        using (_tenant.Change(context.TenantId))
        using (_filters.Disable<ISoftDelete>())
        {
            foreach (var (scope, entry) in entries)
            {
                var predicates = await _tags.GetListAsync(x => x.TenantId == context.TenantId &&
                    (x.StableKey == entry.StableKey || (x.Scope == scope && x.NormalizedName == entry.StableKey.ToUpperInvariant())),
                    includeDetails: false);
                if (predicates.Count > 1)
                    throw new BusinessException(TagsErrorCodes.InvalidRelationDefinition);
                var predicate = predicates.SingleOrDefault();
                if (predicate == null)
                {
                    // A retry after a concurrent insert must never allocate a second predicate identity.
                    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"tags-predicate|{context.TenantId?.ToString("N") ?? "host"}|{entry.StableKey}"));
                    predicate = Tag.CreateRelationPredicate(new Guid(bytes.AsSpan(0, 16)), entry.StableKey, scope, entry.StableKey, context.TenantId);
                    await _tags.InsertAsync(predicate, autoSave: true);
                }
                if (predicate.TenantId != context.TenantId || predicate.IsDeleted || predicate.Kind != TagKind.RelationPredicate ||
                    predicate.StableKey != entry.StableKey || predicate.Scope != scope)
                    throw new BusinessException(TagsErrorCodes.InvalidRelationDefinition);

                var first = await _definitions.FindAsync(x => x.TenantId == context.TenantId &&
                    x.PredicateTagId == predicate.Id && x.Revision == 1);
                if (first == null)
                    await _manager.CreateRevisionAsync(predicate.Id, 0, entry.SourceType, entry.TargetType,
                        symmetric: entry.IsSymmetric, requiresAcyclicGraph: entry.RequiresAcyclicGraph);
                else if (first.StableKey != entry.StableKey || first.SourceEntityType != entry.SourceType ||
                    first.TargetEntityType != entry.TargetType || first.IsSymmetric != entry.IsSymmetric ||
                    first.RequiresAcyclicGraph != entry.RequiresAcyclicGraph || first.AllowsSelfReference)
                    throw new BusinessException(TagsErrorCodes.InvalidRelationDefinition);
            }
        }
    }
}
