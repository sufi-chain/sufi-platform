using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Menus.Menus;

public interface IPublicMenuAppService : IApplicationService
{
    Task<List<MenuItemTreeDto>> GetTreeAsync(string contextType, Guid? contextId, string menuName);
    Task<MenuItemDto?> FindItemBySlugAsync(string contextType, Guid? contextId, string menuName, string slug);
}
