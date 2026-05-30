using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.AspNetCore.Mvc;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Controllers;

[Route("api/menu-management/menus")]
public class MenuController : SufiAbpControllerBase, IMenuAppService
{
    private readonly IMenuAppService _service;
    public MenuController(IMenuAppService service) => _service = service;
    [HttpGet("{id}")] public virtual Task<MenuDto> GetAsync(Guid id) => _service.GetAsync(id);
    [HttpGet] public virtual Task<PagedResultDto<MenuListDto>> GetListAsync(GetMenusInput input) => _service.GetListAsync(input);
    [HttpGet("by-name")] public virtual Task<MenuDto> GetByNameAsync(string contextType, Guid? contextId, string name) => _service.GetByNameAsync(contextType, contextId, name);
    [HttpPost] public virtual Task<MenuDto> CreateAsync(CreateMenuDto input) => _service.CreateAsync(input);
    [HttpPut("{id}")] public virtual Task<MenuDto> UpdateAsync(Guid id, UpdateMenuDto input) => _service.UpdateAsync(id, input);
    [HttpDelete("{id}")] public virtual Task DeleteAsync(Guid id) => _service.DeleteAsync(id);
}
