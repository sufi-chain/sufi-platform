using MongoDB.Driver.Linq;
using SufiChain.SufiPlatform.Tags.MongoDB;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tags.Repositories;

public class MongoEntityRelationRepository : MongoDbRepository<ITagsMongoDbContext, EntityRelation, Guid>, IEntityRelationRepository
{
    public MongoEntityRelationRepository(IMongoDbContextProvider<ITagsMongoDbContext> provider) : base(provider) { }

    public virtual async Task<EntityRelation?> FindByRequestIdAsync(Guid requestId, Guid? tenantId,
        CancellationToken cancellationToken = default) =>
        await (await GetQueryableAsync()).FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Revisions.Any(revision => revision.RequestId == requestId),
            cancellationToken);
}
