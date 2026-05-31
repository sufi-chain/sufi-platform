using MongoDB.Driver;
using SufiChain.SufiAbp.MenuManagement.Menus;
using SufiChain.SufiAbp.MenuManagement.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.MenuManagement.Repositories;

public class MongoMenuRepository : MongoDbRepository<IMenuManagementMongoDbContext, Menu, Guid>, IMenuRepository
{
    public MongoMenuRepository(IMongoDbContextProvider<IMenuManagementMongoDbContext> dbContextProvider) : base(dbContextProvider) { }
    public virtual async Task<Menu?> FindByNameAsync(string contextType, Guid? contextId, string name, Guid? tenantId = null, bool includeDetails = true, CancellationToken cancellationToken = default) => await (await GetCollectionAsync(cancellationToken)).Find(x => x.ContextType == contextType && x.ContextId == contextId && x.Name == name && x.TenantId == tenantId).FirstOrDefaultAsync(cancellationToken);
    public virtual async Task<List<Menu>> GetListByContextAsync(string contextType, Guid? contextId, Guid? tenantId = null, bool includeDetails = false, CancellationToken cancellationToken = default) => await (await GetCollectionAsync(cancellationToken)).Find(x => x.ContextType == contextType && x.ContextId == contextId && x.TenantId == tenantId).SortBy(x => x.DisplayName).ToListAsync(cancellationToken);
}
