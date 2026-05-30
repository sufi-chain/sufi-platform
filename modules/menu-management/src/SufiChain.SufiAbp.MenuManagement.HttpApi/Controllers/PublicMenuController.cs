using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AspNetCore.Mvc;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Controllers;

[Route("api/menu-management/public")]
public class PublicMenuController : SufiAbpControllerBase, IPublicMenuAppService
{
    private readonly IPublicMenuAppService _service;
    public PublicMenuController(IPublicMenuAppService service) => _service = service;
    [HttpGet("tree")] public virtual Task<List<MenuItemTreeDto>> GetTreeAsync(string contextType, Guid? contextId, string menuName) => _service.GetTreeAsync(contextType, contextId, menuName);
    [HttpGet("item-by-slug")] public virtual Task<MenuItemDto?> FindItemBySlugAsync(string contextType, Guid? contextId, string menuName, string slug) => _service.FindItemBySlugAsync(contextType, contextId, menuName, slug);
}
