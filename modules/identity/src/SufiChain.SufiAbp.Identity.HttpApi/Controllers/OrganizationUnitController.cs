using Microsoft.AspNetCore.Mvc;
using global::SufiChain.SufiAbp.Identity;
using global::SufiChain.SufiAbp.Identity.OrganizationUnits;
using global::SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.Identity.Controllers;

/// <summary>
/// Controller for organization unit management operations.
/// </summary>
[Area(IdentityRemoteServiceConsts.ModuleName)]
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/identity/organization-units")]
public class OrganizationUnitController : SufiAbpControllerBase, IOrganizationUnitAppService
{
    private readonly IOrganizationUnitAppService _organizationUnitAppService;

    public OrganizationUnitController(IOrganizationUnitAppService organizationUnitAppService)
    {
        _organizationUnitAppService = organizationUnitAppService;
    }

    #region Tree & CRUD

    /// <summary>
    /// Gets the complete organization unit tree.
    /// </summary>
    [HttpGet]
    [Route("tree")]
    public virtual Task<List<OrganizationUnitDto>> GetTreeAsync()
    {
        return _organizationUnitAppService.GetTreeAsync();
    }

    /// <summary>
    /// Gets a single organization unit by ID.
    /// </summary>
    [HttpGet]
    [Route("{id}")]
    public virtual Task<OrganizationUnitDto> GetAsync(Guid id)
    {
        return _organizationUnitAppService.GetAsync(id);
    }

    /// <summary>
    /// Creates a new organization unit.
    /// </summary>
    [HttpPost]
    public virtual Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitDto input)
    {
        return _organizationUnitAppService.CreateAsync(input);
    }

    /// <summary>
    /// Updates an organization unit.
    /// </summary>
    [HttpPut]
    [Route("{id}")]
    public virtual Task<OrganizationUnitDto> UpdateAsync(Guid id, UpdateOrganizationUnitDto input)
    {
        return _organizationUnitAppService.UpdateAsync(id, input);
    }

    /// <summary>
    /// Deletes an organization unit and all its children.
    /// </summary>
    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _organizationUnitAppService.DeleteAsync(id);
    }

    /// <summary>
    /// Moves an organization unit to a new parent.
    /// </summary>
    [HttpPut]
    [Route("move")]
    public virtual Task MoveAsync(MoveOrganizationUnitDto input)
    {
        return _organizationUnitAppService.MoveAsync(input);
    }

    #endregion

    #region Members

    /// <summary>
    /// Gets the members (users) of an organization unit.
    /// </summary>
    [HttpGet]
    [Route("members")]
    public virtual Task<PagedResultDto<OrganizationUnitMemberDto>> GetMembersAsync([FromQuery] GetOrganizationUnitMembersInput input)
    {
        return _organizationUnitAppService.GetMembersAsync(input);
    }

    /// <summary>
    /// Gets users that can be added to an organization unit.
    /// </summary>
    [HttpGet]
    [Route("available-members")]
    public virtual Task<PagedResultDto<OrganizationUnitMemberDto>> GetAvailableMembersAsync([FromQuery] GetOrganizationUnitMembersInput input)
    {
        return _organizationUnitAppService.GetAvailableMembersAsync(input);
    }

    /// <summary>
    /// Adds users to an organization unit.
    /// </summary>
    [HttpPost]
    [Route("members")]
    public virtual Task AddMembersAsync(OrganizationUnitUserInput input)
    {
        return _organizationUnitAppService.AddMembersAsync(input);
    }

    /// <summary>
    /// Removes a user from an organization unit.
    /// </summary>
    [HttpDelete]
    [Route("{organizationUnitId}/members/{userId}")]
    public virtual Task RemoveMemberAsync(Guid organizationUnitId, Guid userId)
    {
        return _organizationUnitAppService.RemoveMemberAsync(organizationUnitId, userId);
    }

    #endregion

    #region Roles

    /// <summary>
    /// Gets the roles assigned to an organization unit.
    /// </summary>
    [HttpGet]
    [Route("roles")]
    public virtual Task<PagedResultDto<OrganizationUnitRoleDto>> GetRolesAsync([FromQuery] GetOrganizationUnitRolesInput input)
    {
        return _organizationUnitAppService.GetRolesAsync(input);
    }

    /// <summary>
    /// Gets roles that can be assigned to an organization unit.
    /// </summary>
    [HttpGet]
    [Route("available-roles")]
    public virtual Task<PagedResultDto<OrganizationUnitRoleDto>> GetAvailableRolesAsync([FromQuery] GetOrganizationUnitRolesInput input)
    {
        return _organizationUnitAppService.GetAvailableRolesAsync(input);
    }

    /// <summary>
    /// Assigns roles to an organization unit.
    /// </summary>
    [HttpPost]
    [Route("roles")]
    public virtual Task AddRolesAsync(OrganizationUnitRoleInput input)
    {
        return _organizationUnitAppService.AddRolesAsync(input);
    }

    /// <summary>
    /// Removes a role from an organization unit.
    /// </summary>
    [HttpDelete]
    [Route("{organizationUnitId}/roles/{roleId}")]
    public virtual Task RemoveRoleAsync(Guid organizationUnitId, Guid roleId)
    {
        return _organizationUnitAppService.RemoveRoleAsync(organizationUnitId, roleId);
    }

    #endregion
}
