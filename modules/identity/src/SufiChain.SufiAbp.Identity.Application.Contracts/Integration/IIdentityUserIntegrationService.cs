using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Users;
using Volo.Abp;
using Volo.Abp.Application.Services;
using IdentityUserData = SufiChain.SufiAbp.Identity.UserData;

namespace SufiChain.SufiAbp.Identity.Integration;

/// <summary>
/// Integration service for module-to-module identity user and role lookup.
/// </summary>
[IntegrationService]
public interface IIdentityUserIntegrationService : IApplicationService
{
    Task<string[]> GetRoleNamesAsync(Guid id);

    Task<IdentityUserData?> FindByIdAsync(Guid id);

    Task<IdentityUserData?> FindByUserNameAsync(string userName);

    Task<ListResultDto<IdentityUserData>> SearchAsync(UserLookupSearchInputDto input);

    Task<ListResultDto<IdentityUserData>> SearchByIdsAsync(Guid[] ids);

    Task<long> GetCountAsync(UserLookupCountInputDto input);

    Task<ListResultDto<RoleData>> SearchRoleAsync(RoleLookupSearchInputDto input);

    Task<ListResultDto<RoleData>> SearchRoleByNamesAsync(string[] names);

    Task<long> GetRoleCountAsync(RoleLookupCountInputDto input);
}
