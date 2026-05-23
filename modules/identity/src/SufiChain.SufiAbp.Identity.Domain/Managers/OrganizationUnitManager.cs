using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiAbp.Identity;

public class OrganizationUnitManager : DomainService
{
    protected IOrganizationUnitRepository OrganizationUnitRepository { get; }
    protected IIdentityRoleRepository IdentityRoleRepository { get; }

    public OrganizationUnitManager(
        IOrganizationUnitRepository organizationUnitRepository,
        IIdentityRoleRepository identityRoleRepository)
    {
        OrganizationUnitRepository = organizationUnitRepository;
        IdentityRoleRepository = identityRoleRepository;
    }

    public virtual async Task<OrganizationUnit> CreateAsync([NotNull] string displayName, Guid? parentId = null, Guid? id = null)
    {
        var organizationUnit = new OrganizationUnit(
            id ?? GuidGenerator.Create(),
            displayName,
            parentId,
            CurrentTenant.Id
        )
        {
            Code = await GetNextChildCodeAsync(parentId)
        };

        return organizationUnit;
    }

    public virtual async Task CreateAsync(OrganizationUnit organizationUnit)
    {
        organizationUnit.Code = await GetNextChildCodeAsync(organizationUnit.ParentId);
        await ValidateOrganizationUnitAsync(organizationUnit);
        await OrganizationUnitRepository.InsertAsync(organizationUnit);
    }

    public virtual async Task UpdateAsync(OrganizationUnit organizationUnit)
    {
        await ValidateOrganizationUnitAsync(organizationUnit);
        await OrganizationUnitRepository.UpdateAsync(organizationUnit);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var children = await FindChildrenAsync(id, recursive: true);
        foreach (var child in children)
        {
            await OrganizationUnitRepository.RemoveAllMembersAsync(child);
            await OrganizationUnitRepository.RemoveAllRolesAsync(child);
            await OrganizationUnitRepository.DeleteAsync(child);
        }

        var organizationUnit = await OrganizationUnitRepository.GetAsync(id);
        await OrganizationUnitRepository.RemoveAllMembersAsync(organizationUnit);
        await OrganizationUnitRepository.RemoveAllRolesAsync(organizationUnit);
        await OrganizationUnitRepository.DeleteAsync(id);
    }

    public virtual async Task<string> GetNextChildCodeAsync(Guid? parentId)
    {
        var lastChild = await GetLastChildOrNullAsync(parentId);
        if (lastChild != null)
        {
            return OrganizationUnit.CalculateNextCode(lastChild.Code);
        }

        var parentCode = parentId != null
            ? await GetCodeAsync(parentId.Value)
            : null;

        return OrganizationUnit.AppendCode(
            parentCode,
            OrganizationUnit.CreateCode(1)
        );
    }

    public virtual async Task<OrganizationUnit?> GetLastChildOrNullAsync(Guid? parentId)
    {
        var children = await OrganizationUnitRepository.GetChildrenAsync(parentId);
        return children.OrderBy(c => c.Code).LastOrDefault();
    }

    public virtual async Task<string> GetCodeAsync(Guid id)
    {
        return (await OrganizationUnitRepository.GetAsync(id)).Code;
    }

    public virtual async Task MoveAsync(Guid id, Guid? parentId)
    {
        var organizationUnit = await OrganizationUnitRepository.GetAsync(id);
        if (organizationUnit.ParentId == parentId)
        {
            return;
        }

        var children = await FindChildrenAsync(id, recursive: true);

        var oldCode = organizationUnit.Code;

        organizationUnit.Code = await GetNextChildCodeAsync(parentId);
        organizationUnit.ParentId = parentId;

        foreach (var child in children)
        {
            child.Code = OrganizationUnit.AppendCode(organizationUnit.Code, OrganizationUnit.GetRelativeCode(child.Code, oldCode));
        }
    }

    public virtual async Task<List<OrganizationUnit>> FindChildrenAsync(Guid? parentId, bool recursive = false)
    {
        if (!recursive)
        {
            return await OrganizationUnitRepository.GetChildrenAsync(parentId);
        }

        if (!parentId.HasValue)
        {
            return await OrganizationUnitRepository.GetListAsync();
        }

        var code = await GetCodeAsync(parentId.Value);

        return await OrganizationUnitRepository.GetAllChildrenWithParentCodeAsync(code, parentId);
    }

    public virtual async Task AddRoleToOrganizationUnitAsync(Guid roleId, Guid ouId)
    {
        await AddRoleToOrganizationUnitAsync(
            await IdentityRoleRepository.GetAsync(roleId),
            await OrganizationUnitRepository.GetAsync(ouId, includeDetails: true));
    }

    public virtual async Task AddRoleToOrganizationUnitAsync(IdentityRole role, OrganizationUnit ou)
    {
        if (ou.Roles.Any(r => r.OrganizationUnitId == ou.Id && r.RoleId == role.Id))
        {
            return;
        }

        ou.AddRole(role.Id);
        await OrganizationUnitRepository.UpdateAsync(ou);
    }

    public virtual async Task RemoveRoleFromOrganizationUnitAsync(Guid roleId, Guid ouId)
    {
        await RemoveRoleFromOrganizationUnitAsync(
            await IdentityRoleRepository.GetAsync(roleId),
            await OrganizationUnitRepository.GetAsync(ouId, includeDetails: true));
    }

    public virtual async Task RemoveRoleFromOrganizationUnitAsync(IdentityRole role, OrganizationUnit organizationUnit)
    {
        organizationUnit.RemoveRole(role.Id);
        await OrganizationUnitRepository.UpdateAsync(organizationUnit);
    }

    protected virtual async Task ValidateOrganizationUnitAsync(OrganizationUnit organizationUnit)
    {
        var siblings = (await FindChildrenAsync(organizationUnit.ParentId))
            .Where(ou => ou.Id != organizationUnit.Id)
            .ToList();

        if (siblings.Any(ou => ou.DisplayName == organizationUnit.DisplayName))
        {
            throw new BusinessException("SufiAbpIdentity:DuplicateOrganizationUnitDisplayName")
                .WithData("0", organizationUnit.DisplayName);
        }
    }
}
