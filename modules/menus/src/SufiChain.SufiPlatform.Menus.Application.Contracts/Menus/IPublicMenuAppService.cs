using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Menus.Menus;

public interface IPublicMenuAppService : IApplicationService
{
    Task<List<PublicMenuItemTreeDto>> GetTreeAsync(string contextType, Guid? contextId, string menuName);

    /// <summary>Active menu tree, or null when the menu is missing, inactive, or not publicly visible.</summary>
    Task<List<PublicMenuItemTreeDto>?> GetTreeByIdAsync(Guid menuId);
    Task<PublicMenuItemDto?> FindItemBySlugAsync(string contextType, Guid? contextId, string menuName, string slug);
}
