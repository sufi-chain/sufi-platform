namespace SufiChain.SufiPlatform.Identity;

public interface IUserRoleFinder
{
    Task<string[]> GetRoleNamesAsync(Guid userId);

    Task<List<UserFinderResult>> SearchUserAsync(string filter, int page = 1);

    Task<List<RoleFinderResult>> SearchRoleAsync(string filter, int page = 1);

    Task<List<UserFinderResult>> SearchUserByIdsAsync(Guid[] ids);

    Task<List<RoleFinderResult>> SearchRoleByNamesAsync(string[] names);
}
