using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.Identity.OrganizationUnits;
using SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;
using SufiChain.SufiAbp.Identity.Permissions;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Identity;

namespace SufiChain.SufiAbp.Identity.OrganizationUnits;

/// <summary>
/// Application service for managing organization units.
/// Uses ABP Identity Domain services for all operations.
/// </summary>
[Authorize(IdentityPermissions.OrganizationUnits.Default)]
public class OrganizationUnitAppService : SufiAbpApplicationService, IOrganizationUnitAppService
{
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IIdentityRoleRepository _roleRepository;

    public OrganizationUnitAppService(
        OrganizationUnitManager organizationUnitManager,
        IOrganizationUnitRepository organizationUnitRepository,
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository)
    {
        _organizationUnitManager = organizationUnitManager;
        _organizationUnitRepository = organizationUnitRepository;
        _userManager = userManager;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    #region Tree & CRUD

    public virtual async Task<List<OrganizationUnitDto>> GetTreeAsync()
    {
        var allOrganizationUnits = await _organizationUnitRepository.GetListAsync(includeDetails: true);
        var rootUnits = allOrganizationUnits.Where(ou => ou.ParentId == null).ToList();
        
        return BuildTree(rootUnits, allOrganizationUnits);
    }

    public virtual async Task<OrganizationUnitDto> GetAsync(Guid id)
    {
        var organizationUnit = await _organizationUnitRepository.GetAsync(id, includeDetails: true);
        return await MapToOrganizationUnitDtoAsync(organizationUnit);
    }

    [Authorize(IdentityPermissions.OrganizationUnits.Create)]
    public virtual async Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitDto input)
    {
        var organizationUnit = new OrganizationUnit(
            GuidGenerator.Create(),
            input.DisplayName,
            input.ParentId,
            CurrentTenant.Id
        );

        await _organizationUnitManager.CreateAsync(organizationUnit);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return await MapToOrganizationUnitDtoAsync(organizationUnit);
    }

    [Authorize(IdentityPermissions.OrganizationUnits.Update)]
    public virtual async Task<OrganizationUnitDto> UpdateAsync(Guid id, UpdateOrganizationUnitDto input)
    {
        var organizationUnit = await _organizationUnitRepository.GetAsync(id);
        organizationUnit.DisplayName = input.DisplayName;

        await _organizationUnitManager.UpdateAsync(organizationUnit);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return await MapToOrganizationUnitDtoAsync(organizationUnit);
    }

    [Authorize(IdentityPermissions.OrganizationUnits.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _organizationUnitManager.DeleteAsync(id);
    }

    public virtual async Task MoveAsync(MoveOrganizationUnitDto input)
    {
        await _organizationUnitManager.MoveAsync(input.Id, input.NewParentId);
    }

    #endregion

    #region Members

    public virtual async Task<PagedResultDto<OrganizationUnitMemberDto>> GetMembersAsync(GetOrganizationUnitMembersInput input)
    {
        var organizationUnit = await _organizationUnitRepository.GetAsync(input.OrganizationUnitId);

        var totalCount = await _organizationUnitRepository.GetMembersCountAsync(
            organizationUnit,
            filter: input.Filter
        );

        var members = await _organizationUnitRepository.GetMembersAsync(
            organizationUnit,
            sorting: input.Sorting,
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            filter: input.Filter
        );

        var items = ObjectMapper.Map<List<IdentityUser>, List<OrganizationUnitMemberDto>>(members);

        return new PagedResultDto<OrganizationUnitMemberDto>(totalCount, items);
    }

    public virtual async Task<PagedResultDto<OrganizationUnitMemberDto>> GetAvailableMembersAsync(GetOrganizationUnitMembersInput input)
    {
        var organizationUnit = await _organizationUnitRepository.GetAsync(input.OrganizationUnitId);

        var totalCount = await _organizationUnitRepository.GetUnaddedUsersCountAsync(
            organizationUnit,
            filter: input.Filter
        );

        var users = await _organizationUnitRepository.GetUnaddedUsersAsync(
            organizationUnit,
            sorting: input.Sorting,
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            filter: input.Filter
        );

        var items = ObjectMapper.Map<List<IdentityUser>, List<OrganizationUnitMemberDto>>(users);

        return new PagedResultDto<OrganizationUnitMemberDto>(totalCount, items);
    }

    [Authorize(IdentityPermissions.OrganizationUnits.ManageMembers)]
    public virtual async Task AddMembersAsync(OrganizationUnitUserInput input)
    {
        foreach (var userId in input.UserIds)
        {
            await _userManager.AddToOrganizationUnitAsync(userId, input.OrganizationUnitId);
        }
    }

    [Authorize(IdentityPermissions.OrganizationUnits.ManageMembers)]
    public virtual async Task RemoveMemberAsync(Guid organizationUnitId, Guid userId)
    {
        await _userManager.RemoveFromOrganizationUnitAsync(userId, organizationUnitId);
    }

    #endregion

    #region Roles

    public virtual async Task<PagedResultDto<OrganizationUnitRoleDto>> GetRolesAsync(GetOrganizationUnitRolesInput input)
    {
        var organizationUnit = await _organizationUnitRepository.GetAsync(input.OrganizationUnitId, includeDetails: true);

        var totalCount = await _organizationUnitRepository.GetRolesCountAsync(organizationUnit);

        var roles = await _organizationUnitRepository.GetRolesAsync(
            organizationUnit,
            sorting: input.Sorting,
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount
        );

        var items = ObjectMapper.Map<List<IdentityRole>, List<OrganizationUnitRoleDto>>(roles);

        return new PagedResultDto<OrganizationUnitRoleDto>(totalCount, items);
    }

    public virtual async Task<PagedResultDto<OrganizationUnitRoleDto>> GetAvailableRolesAsync(GetOrganizationUnitRolesInput input)
    {
        var organizationUnit = await _organizationUnitRepository.GetAsync(input.OrganizationUnitId, includeDetails: true);

        var totalCount = await _organizationUnitRepository.GetUnaddedRolesCountAsync(
            organizationUnit,
            filter: input.Filter
        );

        var roles = await _organizationUnitRepository.GetUnaddedRolesAsync(
            organizationUnit,
            sorting: input.Sorting,
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            filter: input.Filter
        );

        var items = ObjectMapper.Map<List<IdentityRole>, List<OrganizationUnitRoleDto>>(roles);

        return new PagedResultDto<OrganizationUnitRoleDto>(totalCount, items);
    }

    [Authorize(IdentityPermissions.OrganizationUnits.ManageRoles)]
    public virtual async Task AddRolesAsync(OrganizationUnitRoleInput input)
    {
        foreach (var roleId in input.RoleIds)
        {
            await _organizationUnitManager.AddRoleToOrganizationUnitAsync(roleId, input.OrganizationUnitId);
        }
    }

    [Authorize(IdentityPermissions.OrganizationUnits.ManageRoles)]
    public virtual async Task RemoveRoleAsync(Guid organizationUnitId, Guid roleId)
    {
        await _organizationUnitManager.RemoveRoleFromOrganizationUnitAsync(roleId, organizationUnitId);
    }

    #endregion

    #region Private Methods

    private List<OrganizationUnitDto> BuildTree(
        List<OrganizationUnit> rootUnits,
        List<OrganizationUnit> allUnits)
    {
        var result = new List<OrganizationUnitDto>();

        foreach (var root in rootUnits.OrderBy(ou => ou.Code))
        {
            var dto = MapToOrganizationUnitDto(root);
            dto.Children = BuildChildren(root.Id, allUnits);
            result.Add(dto);
        }

        return result;
    }

    private List<OrganizationUnitDto> BuildChildren(Guid parentId, List<OrganizationUnit> allUnits)
    {
        var children = allUnits.Where(ou => ou.ParentId == parentId).OrderBy(ou => ou.Code).ToList();
        var result = new List<OrganizationUnitDto>();

        foreach (var child in children)
        {
            var dto = MapToOrganizationUnitDto(child);
            dto.Children = BuildChildren(child.Id, allUnits);
            result.Add(dto);
        }

        return result;
    }

    private OrganizationUnitDto MapToOrganizationUnitDto(OrganizationUnit organizationUnit)
    {
        return new OrganizationUnitDto
        {
            Id = organizationUnit.Id,
            ParentId = organizationUnit.ParentId,
            Code = organizationUnit.Code,
            DisplayName = organizationUnit.DisplayName,
            MemberCount = 0, // Will be filled by tree loading or detail view
            RoleCount = organizationUnit.Roles?.Count ?? 0,
            Children = new List<OrganizationUnitDto>()
        };
    }

    private async Task<OrganizationUnitDto> MapToOrganizationUnitDtoAsync(OrganizationUnit organizationUnit)
    {
        var memberCount = await _organizationUnitRepository.GetMembersCountAsync(organizationUnit);
        var roleCount = await _organizationUnitRepository.GetRolesCountAsync(organizationUnit);

        return new OrganizationUnitDto
        {
            Id = organizationUnit.Id,
            ParentId = organizationUnit.ParentId,
            Code = organizationUnit.Code,
            DisplayName = organizationUnit.DisplayName,
            MemberCount = memberCount,
            RoleCount = roleCount,
            Children = new List<OrganizationUnitDto>()
        };
    }

    #endregion
}
