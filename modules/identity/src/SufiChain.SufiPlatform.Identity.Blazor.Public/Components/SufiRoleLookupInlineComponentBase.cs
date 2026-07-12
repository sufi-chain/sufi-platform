using SufiChain.SufiPlatform.Identity;

namespace SufiChain.SufiPlatform.Identity.Blazor.Public.Components;

public abstract class SufiRoleLookupInlineComponentBase : IdentityPublicComponentBase
{
    protected const int DefaultMaxResultCount = 20;

    protected IIdentityRoleAppService RoleAppService => LazyGetRequiredService(ref _roleAppService);
    private IIdentityRoleAppService? _roleAppService;

    protected static string FormatRoleLabel(IdentityRoleDto role) => role.Name;

    protected virtual async Task<List<IdentityRoleDto>> SearchRolesAsync(string? filter, int maxResultCount = DefaultMaxResultCount)
    {
        var result = await RoleAppService.GetListAsync(new GetIdentityRolesInput
        {
            Filter = filter,
            SkipCount = 0,
            MaxResultCount = maxResultCount
        });

        return result.Items.ToList();
    }

    protected virtual async Task<IdentityRoleDto?> TryGetRoleAsync(Guid roleId)
    {
        try
        {
            return await RoleAppService.GetAsync(roleId);
        }
        catch
        {
            return null;
        }
    }
}
