using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Menus.Controllers;

[Area(MenusConsts.ModuleName)]
[RemoteService(Name = MenusConsts.ModuleName)]
[Route("api/menus/public")]
public class PublicMenuController : SufiControllerBase, IPublicMenuAppService
{
    private readonly IPublicMenuAppService _service;
    public PublicMenuController(IPublicMenuAppService service) => _service = service;
    [HttpGet("tree")] public virtual Task<List<MenuItemTreeDto>> GetTreeAsync(string contextType, Guid? contextId, string menuName) => _service.GetTreeAsync(contextType, contextId, menuName);
    [HttpGet("item-by-slug")] public virtual Task<MenuItemDto?> FindItemBySlugAsync(string contextType, Guid? contextId, string menuName, string slug) => _service.FindItemBySlugAsync(contextType, contextId, menuName, slug);
}
