using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Identity;

[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
public interface IIdentityUserAppService : IApplicationService
{
    Task<IdentityUserDto> GetAsync(Guid id);

    Task<PagedResultDto<IdentityUserDto>> GetListAsync(GetIdentityUsersInput input);

    Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input);

    Task<IdentityUserDto> UpdateAsync(Guid id, IdentityUserUpdateDto input);

    Task DeleteAsync(Guid id);

    Task<ListResultDto<IdentityRoleDto>> GetRolesAsync(Guid id);

    Task<ListResultDto<IdentityRoleDto>> GetAssignableRolesAsync();

    Task UpdateRolesAsync(Guid id, IdentityUserUpdateRolesDto input);

    Task<IdentityUserDto> FindByUsernameAsync(string userName);

    Task<IdentityUserDto> FindByEmailAsync(string email);
}
