using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.MenuManagement.Menus;

public interface IMenuRepository : IRepository<Menu, Guid>
{
    Task<Menu?> FindByNameAsync(string contextType, Guid? contextId, string name, Guid? tenantId = null, bool includeDetails = true, CancellationToken cancellationToken = default);
    Task<List<Menu>> GetListByContextAsync(string contextType, Guid? contextId, Guid? tenantId = null, bool includeDetails = false, CancellationToken cancellationToken = default);
}
