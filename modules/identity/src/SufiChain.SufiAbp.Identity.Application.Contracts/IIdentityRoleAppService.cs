using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.Identity;

[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
public interface IIdentityRoleAppService : IApplicationService
{
    Task<ListResultDto<IdentityRoleDto>> GetAllListAsync();

    Task<PagedResultDto<IdentityRoleDto>> GetListAsync(GetIdentityRolesInput input);

    Task<IdentityRoleDto> GetAsync(Guid id);

    Task<IdentityRoleDto> CreateAsync(IdentityRoleCreateDto input);

    Task<IdentityRoleDto> UpdateAsync(Guid id, IdentityRoleUpdateDto input);

    Task DeleteAsync(Guid id);
}
