using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.Menus.Menus;

public interface IMenuItemRepository : IRepository<MenuItem, Guid>
{
    Task<MenuItem?> FindBySlugAsync(Guid menuId, string slug, Guid? tenantId = null, bool includeDetails = true, CancellationToken cancellationToken = default);
    Task<List<MenuItem>> GetTreeItemsAsync(Guid menuId, Guid? tenantId = null, bool includeDetails = false, CancellationToken cancellationToken = default);
    Task<List<MenuItem>> GetChildrenAsync(Guid menuId, Guid? parentId, Guid? tenantId = null, bool includeDetails = false, CancellationToken cancellationToken = default);
    Task<List<MenuItem>> GetByTargetAsync(string targetType, Guid targetId, Guid? tenantId = null, CancellationToken cancellationToken = default);
}
