using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Menus.Menus;

public interface IMenuAppService : IApplicationService
{
    Task<MenuDto> GetAsync(Guid id);
    Task<PagedResultDto<MenuListDto>> GetListAsync(GetMenusInput input);
    Task<MenuDto> GetByNameAsync(string contextType, Guid? contextId, string name);
    Task<MenuDto> CreateAsync(CreateMenuDto input);
    Task<MenuDto> UpdateAsync(Guid id, UpdateMenuDto input);
    Task DeleteAsync(Guid id);
}
