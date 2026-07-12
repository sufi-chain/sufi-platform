using System.Linq.Dynamic.Core;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.SufiAI.MongoDB;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class MongoWorkspaceRepository : MongoDbRepository<AIMongoDbContext, Workspace, Guid>, IWorkspaceRepository
{
    public MongoWorkspaceRepository(IMongoDbContextProvider<AIMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Workspace?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var queryable = await GetMongoQueryableAsync(cancellationToken);
        return await queryable.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<List<Workspace>> GetListAsync(
        string? filter = null,
        int skipCount = 0,
        int maxResultCount = 10,
        string sorting = "Name",
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetMongoQueryableAsync(cancellationToken);
        
        return await queryable
            .WhereIf(!string.IsNullOrWhiteSpace(filter), 
                x => x.Name.Contains(filter!) || x.Model.Contains(filter!))
            .OrderBy(sorting)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default)
    {
        var queryable = await GetMongoQueryableAsync(cancellationToken);
        
        return await queryable
            .WhereIf(!string.IsNullOrWhiteSpace(filter), 
                x => x.Name.Contains(filter!) || x.Model.Contains(filter!))
            .LongCountAsync(cancellationToken);
    }
}
