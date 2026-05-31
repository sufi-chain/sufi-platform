using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.MenuManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.MenuManagement.Menus;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.MenuManagement.Repositories;

public class EfCoreMenuRepository : EfCoreRepository<IMenuManagementDbContext, Menu, Guid>, IMenuRepository
{
    public EfCoreMenuRepository(IDbContextProvider<IMenuManagementDbContext> dbContextProvider) : base(dbContextProvider) { }
    public virtual async Task<Menu?> FindByNameAsync(string contextType, Guid? contextId, string name, Guid? tenantId = null, bool includeDetails = true, CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync()).FirstOrDefaultAsync(x => x.ContextType == contextType && x.ContextId == contextId && x.Name == name && x.TenantId == tenantId, cancellationToken);
    }
    public virtual async Task<List<Menu>> GetListByContextAsync(string contextType, Guid? contextId, Guid? tenantId = null, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync()).Where(x => x.ContextType == contextType && x.ContextId == contextId && x.TenantId == tenantId).OrderBy(x => x.DisplayName).ToListAsync(cancellationToken);
    }
}
