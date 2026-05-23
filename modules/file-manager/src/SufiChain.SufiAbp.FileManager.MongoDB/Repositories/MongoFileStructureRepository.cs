using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SufiChain.SufiAbp.FileManager.FileStructures;
using SufiChain.SufiAbp.FileManager.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.FileManager.Repositories;

public class MongoFileStructureRepository :
    MongoDbRepository<IFileManagerMongoDbContext, FileStructure, Guid>,
    IFileStructureRepository
{
    public MongoFileStructureRepository(IMongoDbContextProvider<IFileManagerMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<FileStructure?> FindByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
    }

    public async Task<bool> KeyExistsAsync(
        string key,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync(cancellationToken);
        query = query.Where(x => x.Key == key);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
