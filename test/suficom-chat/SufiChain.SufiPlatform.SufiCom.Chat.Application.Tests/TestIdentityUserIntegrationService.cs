using System.Collections.Concurrent;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Integration;
using SufiChain.SufiPlatform.Users;
using IdentityUserData = SufiChain.SufiPlatform.Identity.UserData;

namespace SufiChain.SufiPlatform.SufiCom.Chat;

/// <summary>
/// In-memory identity lookup for chat tests. The chat host does not load the Identity module,
/// so tests seed the users and roles they need through <see cref="AddUser"/> and <see cref="AddRole"/>.
/// </summary>
public class TestIdentityUserIntegrationService : IIdentityUserIntegrationService
{
    private readonly ConcurrentDictionary<Guid, IdentityUserData> _users = new();
    private readonly ConcurrentDictionary<Guid, string[]> _userRoles = new();
    private readonly ConcurrentDictionary<string, RoleData> _roles = new(StringComparer.OrdinalIgnoreCase);

    public IdentityUserData AddUser(Guid id, string userName, string? name = null, string? surname = null, params string[] roles)
    {
        var user = new IdentityUserData
        {
            Id = id,
            UserName = userName,
            Name = name,
            Surname = surname,
            IsActive = true
        };
        _users[id] = user;
        _userRoles[id] = roles;
        return user;
    }

    public RoleData AddRole(string name)
    {
        var role = new RoleData { Id = Guid.NewGuid(), Name = name };
        _roles[name] = role;
        return role;
    }

    public Task<string[]> GetRoleNamesAsync(Guid id)
        => Task.FromResult(_userRoles.TryGetValue(id, out var roles) ? roles : Array.Empty<string>());

    public Task<IdentityUserData?> FindByIdAsync(Guid id)
        => Task.FromResult(_users.TryGetValue(id, out var user) ? user : null);

    public Task<IdentityUserData?> FindByUserNameAsync(string userName)
        => Task.FromResult(_users.Values.FirstOrDefault(u =>
            string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase)));

    public Task<ListResultDto<IdentityUserData>> SearchAsync(UserLookupSearchInputDto input)
        => Task.FromResult(new ListResultDto<IdentityUserData>(FilterUsers(input.Filter)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList()));

    public Task<ListResultDto<IdentityUserData>> SearchByIdsAsync(Guid[] ids)
        => Task.FromResult(new ListResultDto<IdentityUserData>(ids
            .Where(_users.ContainsKey)
            .Select(id => _users[id])
            .ToList()));

    public Task<long> GetCountAsync(UserLookupCountInputDto input)
        => Task.FromResult((long)FilterUsers(input.Filter).Count());

    public Task<ListResultDto<RoleData>> SearchRoleAsync(RoleLookupSearchInputDto input)
        => Task.FromResult(new ListResultDto<RoleData>(FilterRoles(input.Filter)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList()));

    public Task<ListResultDto<RoleData>> SearchRoleByNamesAsync(string[] names)
        => Task.FromResult(new ListResultDto<RoleData>(names
            .Where(_roles.ContainsKey)
            .Select(name => _roles[name])
            .ToList()));

    public Task<long> GetRoleCountAsync(RoleLookupCountInputDto input)
        => Task.FromResult((long)FilterRoles(input.Filter).Count());

    private IEnumerable<IdentityUserData> FilterUsers(string? filter)
        => string.IsNullOrWhiteSpace(filter)
            ? _users.Values
            : _users.Values.Where(u =>
                Contains(u.UserName, filter) || Contains(u.Name, filter) ||
                Contains(u.Surname, filter) || Contains(u.Email, filter));

    private IEnumerable<RoleData> FilterRoles(string? filter)
        => string.IsNullOrWhiteSpace(filter)
            ? _roles.Values
            : _roles.Values.Where(r => Contains(r.Name, filter));

    private static bool Contains(string? value, string filter)
        => value?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true;
}
