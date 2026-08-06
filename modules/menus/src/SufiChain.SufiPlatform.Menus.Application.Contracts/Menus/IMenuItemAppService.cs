using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Menus.Menus;

public interface IMenuItemAppService : IApplicationService
{
    Task<MenuItemDto> GetAsync(Guid id);
    Task<PagedResultDto<MenuItemDto>> GetListAsync(GetMenuItemsInput input);
    Task<List<MenuItemTreeDto>> GetTreeAsync(GetMenuTreeInput input);
    Task<MenuItemDto?> FindBySlugAsync(Guid menuId, string slug);
    Task<MenuItemDto> CreateAsync(CreateMenuItemDto input);
    Task<MenuItemDto> UpdateAsync(Guid id, UpdateMenuItemDto input);
    Task DeleteAsync(Guid id);
    Task<MenuItemDto> MoveAsync(Guid id, MoveMenuItemDto input);
    Task<MenuItemDto> ReorderAsync(Guid id, int displayOrder);
}
