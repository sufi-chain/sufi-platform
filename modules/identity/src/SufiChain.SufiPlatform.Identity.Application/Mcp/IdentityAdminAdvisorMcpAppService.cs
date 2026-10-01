using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Identity.Dtos;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Hooshvare;
using SufiChain.SufiPlatform.Identity.OrganizationUnits;
using SufiChain.SufiPlatform.Identity.OrganizationUnits.Dtos;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.Identity.Mcp;

[Authorize]
public class IdentityAdminAdvisorMcpAppService : SufiApplicationService
{
    private const int MaxResults = 50;

    protected IIdentityUserAppService Users { get; }
    protected IIdentityRoleAppService Roles { get; }
    protected IOrganizationUnitAppService OrganizationUnits { get; }
    protected IIdentitySecurityLogAppService SecurityLogs { get; }

    public IdentityAdminAdvisorMcpAppService(
        IIdentityUserAppService users,
        IIdentityRoleAppService roles,
        IOrganizationUnitAppService organizationUnits,
        IIdentitySecurityLogAppService securityLogs)
    {
        Users = users;
        Roles = roles;
        OrganizationUnits = organizationUnits;
        SecurityLogs = securityLogs;
    }

    [SufiAiMcpTool(IdentityAdminAdvisorHooshvareKeys.Tools.SearchUsers,
        "Searches users. Omits passwords, tokens, hashes, and private claims.", ReadOnly = true)]
    [Authorize(IdentityPermissions.Users.Default)]
    public virtual async Task<object> SearchUsersAsync(string? filter = null)
    {
        var page = await Users.GetListAsync(new GetIdentityUsersInput
        {
            Filter = filter,
            MaxResultCount = MaxResults
        });
        return new
        {
            page.TotalCount,
            Items = page.Items.Select(MapUser).ToList()
        };
    }

    [SufiAiMcpTool(IdentityAdminAdvisorHooshvareKeys.Tools.GetUser,
        "Returns one user without passwords, tokens, hashes, or private claims.", ReadOnly = true)]
    [Authorize(IdentityPermissions.Users.Default)]
    public virtual async Task<object> GetUserAsync(Guid id) => MapUser(await Users.GetAsync(id));

    [SufiAiMcpTool(IdentityAdminAdvisorHooshvareKeys.Tools.GetUserRoles,
        "Lists roles assigned to one user. Does not change role grants.", ReadOnly = true)]
    [Authorize(IdentityPermissions.Users.Default)]
    public virtual async Task<object> GetUserRolesAsync(Guid userId)
    {
        var roles = await Users.GetRolesAsync(userId);
        return roles.Items.Select(MapRole).ToList();
    }

    [SufiAiMcpTool(IdentityAdminAdvisorHooshvareKeys.Tools.ListRoles,
        "Lists roles. Does not create or change them.", ReadOnly = true)]
    [Authorize(IdentityPermissions.Roles.Default)]
    public virtual async Task<object> ListRolesAsync()
    {
        var roles = await Roles.GetAllListAsync();
        return roles.Items.Take(MaxResults).Select(MapRole).ToList();
    }

    [SufiAiMcpTool(IdentityAdminAdvisorHooshvareKeys.Tools.GetOuTree,
        "Returns the organization unit tree. Does not move or edit units.", ReadOnly = true)]
    [Authorize(IdentityPermissions.OrganizationUnits.Default)]
    public virtual async Task<object> GetOrganizationUnitTreeAsync()
    {
        var tree = await OrganizationUnits.GetTreeAsync();
        return tree.Select(MapUnit).ToList();
    }

    [SufiAiMcpTool(IdentityAdminAdvisorHooshvareKeys.Tools.SearchSecurityLogs,
        "Searches security logs. Extra properties that may hold tokens are omitted.", ReadOnly = true)]
    [Authorize(IdentityPermissions.SecurityLogs.Default)]
    public virtual async Task<object> SearchSecurityLogsAsync(Guid? userId = null, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var page = await SecurityLogs.GetListAsync(new GetSecurityLogListInput
        {
            UserId = userId,
            StartTime = fromUtc,
            EndTime = toUtc,
            MaxResultCount = MaxResults
        });
        return new
        {
            page.TotalCount,
            Items = page.Items.Select(item => new
            {
                item.Id,
                item.UserId,
                item.UserName,
                item.Action,
                item.Identity,
                item.CreationTime,
                item.ClientIpAddress
            }).ToList()
        };
    }

    private static object MapUser(IdentityUserDto user) => new
    {
        user.Id,
        user.UserName,
        user.Name,
        user.Surname,
        user.Email,
        user.EmailConfirmed,
        user.PhoneNumber,
        user.IsActive,
        user.LockoutEnabled,
        user.AccessFailedCount
    };

    private static object MapRole(IdentityRoleDto role) => new
    {
        role.Id,
        role.Name,
        role.IsDefault,
        role.IsPublic,
        role.IsStatic
    };

    private static object MapUnit(OrganizationUnitDto unit) => new
    {
        unit.Id,
        unit.ParentId,
        unit.DisplayName,
        unit.Code,
        unit.MemberCount,
        unit.RoleCount,
        Children = unit.Children.Select(MapUnit).ToList()
    };
}
