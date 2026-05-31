using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.MenuManagement.Menus;
using Volo.Abp;

namespace SufiChain.SufiAbp.MenuManagement.Controllers;

[Area(MenuManagementConsts.ModuleName)]
[RemoteService(Name = MenuManagementConsts.ModuleName)]
[Route("api/menu-management/public")]
public class PublicMenuController : SufiAbpControllerBase, IPublicMenuAppService
{
    private readonly IPublicMenuAppService _service;
    public PublicMenuController(IPublicMenuAppService service) => _service = service;
    [HttpGet("tree")] public virtual Task<List<MenuItemTreeDto>> GetTreeAsync(string contextType, Guid? contextId, string menuName) => _service.GetTreeAsync(contextType, contextId, menuName);
    [HttpGet("item-by-slug")] public virtual Task<MenuItemDto?> FindItemBySlugAsync(string contextType, Guid? contextId, string menuName, string slug) => _service.FindItemBySlugAsync(contextType, contextId, menuName, slug);
}
