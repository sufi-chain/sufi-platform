using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Menus.Menus;

public interface IPublicMenuAppService : IApplicationService
{
    Task<List<MenuItemTreeDto>> GetTreeAsync(string contextType, Guid? contextId, string menuName);

    /// <summary>Active menu tree, or null when the menu is missing or inactive.</summary>
    Task<List<MenuItemTreeDto>?> GetTreeByIdAsync(Guid menuId);
    Task<MenuItemDto?> FindItemBySlugAsync(string contextType, Guid? contextId, string menuName, string slug);
}
