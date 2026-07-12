using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Menus.Controllers;

[Area(MenusConsts.ModuleName)]
[RemoteService(Name = MenusConsts.ModuleName)]
[Route("api/menus/menu-items")]
public class MenuItemController : SufiControllerBase, IMenuItemAppService
{
    private readonly IMenuItemAppService _service;
    public MenuItemController(IMenuItemAppService service) => _service = service;
    [HttpGet("{id}")] public virtual Task<MenuItemDto> GetAsync(Guid id) => _service.GetAsync(id);
    [HttpGet] public virtual Task<PagedResultDto<MenuItemDto>> GetListAsync(GetMenuItemsInput input) => _service.GetListAsync(input);
    [HttpGet("tree")] public virtual Task<List<MenuItemTreeDto>> GetTreeAsync(GetMenuTreeInput input) => _service.GetTreeAsync(input);
    [HttpGet("by-slug")] public virtual Task<MenuItemDto?> FindBySlugAsync(Guid menuId, string slug) => _service.FindBySlugAsync(menuId, slug);
    [HttpPost] public virtual Task<MenuItemDto> CreateAsync(CreateMenuItemDto input) => _service.CreateAsync(input);
    [HttpPut("{id}")] public virtual Task<MenuItemDto> UpdateAsync(Guid id, UpdateMenuItemDto input) => _service.UpdateAsync(id, input);
    [HttpDelete("{id}")] public virtual Task DeleteAsync(Guid id) => _service.DeleteAsync(id);
    [HttpPut("{id}/move")] public virtual Task<MenuItemDto> MoveAsync(Guid id, MoveMenuItemDto input) => _service.MoveAsync(id, input);
    [HttpPut("{id}/reorder")] public virtual Task<MenuItemDto> ReorderAsync(Guid id, int displayOrder) => _service.ReorderAsync(id, displayOrder);
}
