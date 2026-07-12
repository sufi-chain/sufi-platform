using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Users;
using Volo.Abp;
using Volo.Abp.Application.Services;
using IdentityUserData = SufiChain.SufiPlatform.Identity.UserData;

namespace SufiChain.SufiPlatform.Identity.Integration;

/// <summary>
/// Integration service for module-to-module identity user and role lookup.
/// </summary>
[IntegrationService]
public interface IIdentityUserIntegrationService : IApplicationService
{
    Task<string[]> GetRoleNamesAsync(System.Guid id);

    Task<IdentityUserData?> FindByIdAsync(System.Guid id);

    Task<IdentityUserData?> FindByUserNameAsync(string userName);

    Task<ListResultDto<IdentityUserData>> SearchAsync(UserLookupSearchInputDto input);

    Task<ListResultDto<IdentityUserData>> SearchByIdsAsync(System.Guid[] ids);

    Task<long> GetCountAsync(UserLookupCountInputDto input);

    Task<ListResultDto<RoleData>> SearchRoleAsync(RoleLookupSearchInputDto input);

    Task<ListResultDto<RoleData>> SearchRoleByNamesAsync(string[] names);

    Task<long> GetRoleCountAsync(RoleLookupCountInputDto input);
}
