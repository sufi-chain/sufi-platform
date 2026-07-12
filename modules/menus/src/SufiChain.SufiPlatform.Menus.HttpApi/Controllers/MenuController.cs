using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Menus.Controllers;

[Area(MenusConsts.ModuleName)]
[RemoteService(Name = MenusConsts.ModuleName)]
[Route("api/menus/menus")]
public class MenuController : SufiControllerBase, IMenuAppService
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
