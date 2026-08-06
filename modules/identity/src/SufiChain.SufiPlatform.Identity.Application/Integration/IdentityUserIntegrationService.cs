using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Identity.Integration;
using SufiChain.SufiPlatform.Users;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using IdentityUserData = SufiChain.SufiPlatform.Identity.UserData;
using SystemGuid = System.Guid;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityUserIntegrationService : SufiApplicationService, IIdentityUserIntegrationService
{
    protected IUserRoleFinder UserRoleFinder { get; }

    protected IdentityUserRepositoryExternalUserLookupServiceProvider UserLookupServiceProvider { get; }

    protected IIdentityUserRepository UserRepository { get; }

    protected IIdentityRoleRepository RoleRepository { get; }

    public IdentityUserIntegrationService(
        IUserRoleFinder userRoleFinder,
        IdentityUserRepositoryExternalUserLookupServiceProvider userLookupServiceProvider,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository)
    {
        UserRoleFinder = userRoleFinder;
        UserLookupServiceProvider = userLookupServiceProvider;
        UserRepository = userRepository;
        RoleRepository = roleRepository;
    }

    public virtual async Task<string[]> GetRoleNamesAsync(SystemGuid id)
    {
        return await UserRoleFinder.GetRoleNamesAsync(id);
    }

    public virtual async Task<IdentityUserData?> FindByIdAsync(SystemGuid id)
    {
        var userData = await UserLookupServiceProvider.FindByIdAsync(id);
        return userData == null ? null : MapToUserData(userData);
    }

    public virtual async Task<IdentityUserData?> FindByUserNameAsync(string userName)
    {
        var userData = await UserLookupServiceProvider.FindByUserNameAsync(userName);
        return userData == null ? null : MapToUserData(userData);
    }

    public virtual async Task<ListResultDto<IdentityUserData>> SearchAsync(UserLookupSearchInputDto input)
    {
        var users = await UserLookupServiceProvider.SearchAsync(
            input.Sorting,
            input.Filter,
            input.MaxResultCount,
            input.SkipCount);

        return new ListResultDto<IdentityUserData>(
            users
                .Select(MapToUserData)
                .ToList());
    }

    public virtual async Task<ListResultDto<IdentityUserData>> SearchByIdsAsync(SystemGuid[] ids)
    {
        var users = await UserRepository.GetListByIdsAsync(ids);

        return new ListResultDto<IdentityUserData>(
            users
                .Select(u => new IdentityUserData(
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.Name,
                    u.Surname,
                    u.EmailConfirmed,
                    u.PhoneNumber,
                    u.PhoneNumberConfirmed,
                    ToSystemGuidOrNull(u.TenantId),
                    u.IsActive,
                    u.ExtraProperties))
                .ToList());
    }

    public virtual async Task<long> GetCountAsync(UserLookupCountInputDto input)
    {
        return await UserLookupServiceProvider.GetCountAsync(input.Filter);
    }

    public virtual async Task<ListResultDto<RoleData>> SearchRoleAsync(RoleLookupSearchInputDto input)
    {
        using (RoleRepository.DisableTracking())
        {
            var roles = await RoleRepository.GetListAsync(
                sorting: input.Sorting,
                maxResultCount: input.MaxResultCount,
                skipCount: input.SkipCount,
                filter: input.Filter);

            return new ListResultDto<RoleData>(
                roles.Select(r => new RoleData(
                    r.Id,
                    r.Name,
                    r.IsDefault,
                    r.IsStatic,
                    r.IsPublic,
                    r.TenantId,
                    r.ExtraProperties)).ToList());
        }
    }

    public virtual async Task<ListResultDto<RoleData>> SearchRoleByNamesAsync(string[] names)
    {
        using (RoleRepository.DisableTracking())
        {
            var roles = await RoleRepository.GetListAsync(names);

            return new ListResultDto<RoleData>(
                roles.Select(r => new RoleData(
                    r.Id,
                    r.Name,
                    r.IsDefault,
                    r.IsStatic,
                    r.IsPublic,
                    r.TenantId,
                    r.ExtraProperties)).ToList());
        }
    }

    public virtual async Task<long> GetRoleCountAsync(RoleLookupCountInputDto input)
    {
        return await RoleRepository.GetCountAsync(input.Filter);
    }

    protected virtual IdentityUserData MapToUserData(IUserData user)
    {
        return new IdentityUserData(
            user.Id,
            user.UserName,
            user.Email,
            user.Name,
            user.Surname,
            user.EmailConfirmed,
            user.PhoneNumber,
            user.PhoneNumberConfirmed,
            user.TenantId,
            user.IsActive,
            user.ExtraProperties);
    }

    protected virtual SystemGuid? ToSystemGuidOrNull(object? value)
    {
        return value == null
            ? null
            : SystemGuid.Parse(value.ToString()!);
    }
}
