using MongoDB.Driver;
using SufiChain.SufiPlatform.Menus.Menus;
using SufiChain.SufiPlatform.Menus.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Menus.Repositories;

public class MongoMenuItemRepository : MongoDbRepository<IMenusMongoDbContext, MenuItem, Guid>, IMenuItemRepository
{
    public MongoMenuItemRepository(IMongoDbContextProvider<IMenusMongoDbContext> dbContextProvider) : base(dbContextProvider) { }
    public virtual async Task<MenuItem?> FindBySlugAsync(Guid menuId, string slug, Guid? tenantId = null, bool includeDetails = true, CancellationToken cancellationToken = default) => await (await GetCollectionAsync(cancellationToken)).Find(x => x.MenuId == menuId && x.Slug == slug && x.TenantId == tenantId).FirstOrDefaultAsync(cancellationToken);
    public virtual async Task<List<MenuItem>> GetTreeItemsAsync(Guid menuId, Guid? tenantId = null, bool includeDetails = false, CancellationToken cancellationToken = default) => await (await GetCollectionAsync(cancellationToken)).Find(x => x.MenuId == menuId && x.TenantId == tenantId).SortBy(x => x.DisplayOrder).ThenBy(x => x.DisplayName).ToListAsync(cancellationToken);
    public virtual async Task<List<MenuItem>> GetChildrenAsync(Guid menuId, Guid? parentId, Guid? tenantId = null, bool includeDetails = false, CancellationToken cancellationToken = default) => await (await GetCollectionAsync(cancellationToken)).Find(x => x.MenuId == menuId && x.ParentId == parentId && x.TenantId == tenantId).SortBy(x => x.DisplayOrder).ThenBy(x => x.DisplayName).ToListAsync(cancellationToken);
    public virtual async Task<List<MenuItem>> GetByTargetAsync(string targetType, Guid targetId, Guid? tenantId = null, CancellationToken cancellationToken = default) => await (await GetCollectionAsync(cancellationToken)).Find(x => x.TargetType == targetType && x.TargetId == targetId && x.TenantId == tenantId).ToListAsync(cancellationToken);
}
