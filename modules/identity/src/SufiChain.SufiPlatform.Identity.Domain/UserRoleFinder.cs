using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.Identity;

public class UserRoleFinder : IUserRoleFinder, ITransientDependency
{
    protected IIdentityUserRepository IdentityUserRepository { get; }

    protected IIdentityRoleRepository IdentityRoleRepository { get; }

    public UserRoleFinder(
        IIdentityUserRepository identityUserRepository,
        IIdentityRoleRepository identityRoleRepository)
    {
        IdentityUserRepository = identityUserRepository;
        IdentityRoleRepository = identityRoleRepository;
    }

    public virtual async Task<string[]> GetRoleNamesAsync(Guid userId)
    {
        return (await IdentityUserRepository.GetRoleNamesAsync(userId)).ToArray();
    }

    public virtual async Task<List<UserFinderResult>> SearchUserAsync(string filter, int page = 1)
    {
        using (IdentityUserRepository.DisableTracking())
        {
            page = page < 1 ? 1 : page;
            var users = await IdentityUserRepository.GetListAsync(filter: filter, skipCount: (page - 1) * 10, maxResultCount: 10);

            return users
                .Select(user => new UserFinderResult
                {
                    Id = user.Id,
                    UserName = user.UserName
                })
                .ToList();
        }
    }

    public virtual async Task<List<RoleFinderResult>> SearchRoleAsync(string filter, int page = 1)
    {
        using (IdentityRoleRepository.DisableTracking())
        {
            page = page < 1 ? 1 : page;
            var roles = await IdentityRoleRepository.GetListAsync(filter: filter, skipCount: (page - 1) * 10, maxResultCount: 10);

            return roles
                .Select(role => new RoleFinderResult
                {
                    Id = role.Id,
                    RoleName = role.Name
                })
                .ToList();
        }
    }

    public virtual async Task<List<UserFinderResult>> SearchUserByIdsAsync(Guid[] ids)
    {
        using (IdentityUserRepository.DisableTracking())
        {
            var users = await IdentityUserRepository.GetListByIdsAsync(ids);

            return users
                .Select(user => new UserFinderResult
                {
                    Id = user.Id,
                    UserName = user.UserName
                })
                .ToList();
        }
    }

    public virtual async Task<List<RoleFinderResult>> SearchRoleByNamesAsync(string[] names)
    {
        return names
            .Select(name => new RoleFinderResult
            {
                RoleName = name
            })
            .ToList();
    }
}
