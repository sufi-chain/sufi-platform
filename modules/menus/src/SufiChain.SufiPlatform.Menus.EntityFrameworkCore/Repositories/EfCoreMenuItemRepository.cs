using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Menus.EntityFrameworkCore;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Menus.Repositories;

public class EfCoreMenuItemRepository : EfCoreRepository<IMenusDbContext, MenuItem, Guid>, IMenuItemRepository
{
    public EfCoreMenuItemRepository(IDbContextProvider<IMenusDbContext> dbContextProvider) : base(dbContextProvider) { }
    public virtual async Task<MenuItem?> FindBySlugAsync(Guid menuId, string slug, Guid? tenantId = null, bool includeDetails = true, CancellationToken cancellationToken = default) => await (await GetDbSetAsync()).FirstOrDefaultAsync(x => x.MenuId == menuId && x.Slug == slug && x.TenantId == tenantId, cancellationToken);
    public virtual async Task<List<MenuItem>> GetTreeItemsAsync(Guid menuId, Guid? tenantId = null, bool includeDetails = false, CancellationToken cancellationToken = default) => await (await GetDbSetAsync()).Where(x => x.MenuId == menuId && x.TenantId == tenantId).OrderBy(x => x.DisplayOrder).ThenBy(x => x.DisplayName).ToListAsync(cancellationToken);
    public virtual async Task<List<MenuItem>> GetChildrenAsync(Guid menuId, Guid? parentId, Guid? tenantId = null, bool includeDetails = false, CancellationToken cancellationToken = default) => await (await GetDbSetAsync()).Where(x => x.MenuId == menuId && x.ParentId == parentId && x.TenantId == tenantId).OrderBy(x => x.DisplayOrder).ThenBy(x => x.DisplayName).ToListAsync(cancellationToken);
    public virtual async Task<List<MenuItem>> GetByTargetAsync(string targetType, Guid targetId, Guid? tenantId = null, CancellationToken cancellationToken = default) => await (await GetDbSetAsync()).Where(x => x.TargetType == targetType && x.TargetId == targetId && x.TenantId == tenantId).ToListAsync(cancellationToken);
}
