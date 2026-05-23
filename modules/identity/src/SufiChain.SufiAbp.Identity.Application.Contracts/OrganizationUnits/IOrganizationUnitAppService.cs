using SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.Identity.OrganizationUnits;

/// <summary>
/// Application service interface for managing organization units.
/// </summary>
public interface IOrganizationUnitAppService : IApplicationService
{
    #region Tree & CRUD

    /// <summary>
    /// Gets the complete organization unit tree.
    /// </summary>
    Task<List<OrganizationUnitDto>> GetTreeAsync();

    /// <summary>
    /// Gets a single organization unit by ID.
    /// </summary>
    Task<OrganizationUnitDto> GetAsync(Guid id);

    /// <summary>
    /// Creates a new organization unit.
    /// </summary>
    Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitDto input);

    /// <summary>
    /// Updates an organization unit.
    /// </summary>
    Task<OrganizationUnitDto> UpdateAsync(Guid id, UpdateOrganizationUnitDto input);

    /// <summary>
    /// Deletes an organization unit and all its children.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Moves an organization unit to a new parent.
    /// </summary>
    Task MoveAsync(MoveOrganizationUnitDto input);

    #endregion

    #region Members

    /// <summary>
    /// Gets the members (users) of an organization unit.
    /// </summary>
    Task<PagedResultDto<OrganizationUnitMemberDto>> GetMembersAsync(GetOrganizationUnitMembersInput input);

    /// <summary>
    /// Gets users that can be added to an organization unit.
    /// </summary>
    Task<PagedResultDto<OrganizationUnitMemberDto>> GetAvailableMembersAsync(GetOrganizationUnitMembersInput input);

    /// <summary>
    /// Adds users to an organization unit.
    /// </summary>
    Task AddMembersAsync(OrganizationUnitUserInput input);

    /// <summary>
    /// Removes a user from an organization unit.
    /// </summary>
    Task RemoveMemberAsync(Guid organizationUnitId, Guid userId);

    #endregion

    #region Roles

    /// <summary>
    /// Gets the roles assigned to an organization unit.
    /// </summary>
    Task<PagedResultDto<OrganizationUnitRoleDto>> GetRolesAsync(GetOrganizationUnitRolesInput input);

    /// <summary>
    /// Gets roles that can be assigned to an organization unit.
    /// </summary>
    Task<PagedResultDto<OrganizationUnitRoleDto>> GetAvailableRolesAsync(GetOrganizationUnitRolesInput input);

    /// <summary>
    /// Assigns roles to an organization unit.
    /// </summary>
    Task AddRolesAsync(OrganizationUnitRoleInput input);

    /// <summary>
    /// Removes a role from an organization unit.
    /// </summary>
    Task RemoveRoleAsync(Guid organizationUnitId, Guid roleId);

    #endregion
}
