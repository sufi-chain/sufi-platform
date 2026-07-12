using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Permissions.MongoDB;

public class MongoPermissionDefinitionRecordRepository :
    MongoDbRepository<IPermissionsMongoDbContext, PermissionDefinitionRecord, Guid>,
    IPermissionDefinitionRecordRepository
{
    public MongoPermissionDefinitionRecordRepository(
        IMongoDbContextProvider<IPermissionsMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<PermissionDefinitionRecord> FindByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        cancellationToken = GetCancellationToken(cancellationToken);
        return await (await GetQueryableAsync(cancellationToken))
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(
                s => s.Name == name,
                cancellationToken
            );
    }
}