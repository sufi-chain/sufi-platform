using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.Identity.EntityFrameworkCore;

public class EfCoreOrganizationUnitRepository : EfCoreRepository<ISufiAbpIdentityDbContext, OrganizationUnit, Guid>, IOrganizationUnitRepository
{
    public EfCoreOrganizationUnitRepository(IDbContextProvider<ISufiAbpIdentityDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<List<OrganizationUnit>> GetChildrenAsync(
        Guid? parentId,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .Where(ou => ou.ParentId == parentId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<OrganizationUnit>> GetAllChildrenWithParentCodeAsync(
        string code,
        Guid? parentId,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .Where(ou => ou.Code.StartsWith(code) && ou.Id != parentId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<OrganizationUnit>> GetListAsync(
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(OrganizationUnit.DisplayName) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<OrganizationUnit>> GetListAsync(
        IEnumerable<Guid> ids,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .Where(ou => ids.Contains(ou.Id))
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<OrganizationUnit?> GetAsync(
        string displayName,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .FirstOrDefaultAsync(
                ou => ou.DisplayName == displayName,
                GetCancellationToken(cancellationToken)
            );
    }

    public virtual async Task<long> GetCountAsync(
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<OrganizationUnit>> GetListByRoleIdAsync(
        Guid roleId,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .Where(ou => ou.Roles.Any(r => r.RoleId == roleId))
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<OrganizationUnit>> GetListByDisplayNamesAsync(
        string[] displayNames,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .Where(ou => displayNames.Contains(ou.DisplayName))
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityRole>> GetRolesAsync(
        Guid id,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from ouRole in dbContext.Set<OrganizationUnitRole>()
                    join role in dbContext.Roles.IncludeDetails(includeDetails) on ouRole.RoleId equals role.Id
                    where ouRole.OrganizationUnitId == id
                    select role;

        return await query
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(IdentityRole.Name) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<int> GetRolesCountAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<OrganizationUnitRole>()
            .Where(ouRole => ouRole.OrganizationUnitId == id)
            .CountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityRole>> GetRolesAsync(
        OrganizationUnit organizationUnit,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await GetRolesAsync(organizationUnit.Id, sorting, maxResultCount, skipCount, includeDetails, cancellationToken);
    }

    public virtual async Task<List<IdentityRole>> GetRolesAsync(
        Guid[] organizationUnitIds,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from ouRole in dbContext.Set<OrganizationUnitRole>()
                    join role in dbContext.Roles.IncludeDetails(includeDetails) on ouRole.RoleId equals role.Id
                    where organizationUnitIds.Contains(ouRole.OrganizationUnitId)
                    select role;

        return await query
            .Distinct()
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(IdentityRole.Name) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual Task<int> GetRolesCountAsync(
        OrganizationUnit organizationUnit,
        CancellationToken cancellationToken = default)
    {
        return GetRolesCountAsync(organizationUnit.Id, cancellationToken);
    }

    public virtual async Task<List<IdentityRole>> GetUnaddedRolesAsync(
        OrganizationUnit organizationUnit,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string? filter = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var roleIds = organizationUnit.Roles.Select(r => r.RoleId).ToList();
        return await (await GetDbContextAsync()).Roles
            .IncludeDetails(includeDetails)
            .Where(r => !roleIds.Contains(r.Id))
            .WhereIf(!filter.IsNullOrWhiteSpace(), r => r.Name.Contains(filter!))
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(IdentityRole.Name) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<int> GetUnaddedRolesCountAsync(
        OrganizationUnit organizationUnit,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var roleIds = organizationUnit.Roles.Select(r => r.RoleId).ToList();
        return await (await GetDbContextAsync()).Roles
            .Where(r => !roleIds.Contains(r.Id))
            .WhereIf(!filter.IsNullOrWhiteSpace(), r => r.Name.Contains(filter!))
            .CountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityUser>> GetMembersAsync(
        Guid id,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string? filter = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from userOU in dbContext.Set<IdentityUserOrganizationUnit>()
                    join user in dbContext.Users.IncludeDetails(includeDetails) on userOU.UserId equals user.Id
                    where userOU.OrganizationUnitId == id
                    select user;

        query = query.WhereIf(
            !filter.IsNullOrWhiteSpace(),
            u =>
                u.UserName!.Contains(filter!) ||
                u.Email.Contains(filter!) ||
                (u.Name != null && u.Name.Contains(filter!)) ||
                (u.Surname != null && u.Surname.Contains(filter!))
        );

        return await query
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(IdentityUser.UserName) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<int> GetMembersCountAsync(
        Guid id,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from userOU in dbContext.Set<IdentityUserOrganizationUnit>()
                    join user in dbContext.Users on userOU.UserId equals user.Id
                    where userOU.OrganizationUnitId == id
                    select user;

        query = query.WhereIf(
            !filter.IsNullOrWhiteSpace(),
            u =>
                u.UserName!.Contains(filter!) ||
                u.Email.Contains(filter!) ||
                (u.Name != null && u.Name.Contains(filter!)) ||
                (u.Surname != null && u.Surname.Contains(filter!))
        );

        return await query.CountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual Task<List<IdentityUser>> GetMembersAsync(
        OrganizationUnit organizationUnit,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string? filter = null,
        bool includeChildren = false,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return GetMembersAsync(organizationUnit.Id, sorting, maxResultCount, skipCount, filter, includeDetails, cancellationToken);
    }

    public virtual async Task<List<Guid>> GetMemberIdsAsync(
        Guid id,
        bool includeChildren = false,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<IdentityUserOrganizationUnit>()
            .Where(userOU => userOU.OrganizationUnitId == id)
            .Select(userOU => userOU.UserId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual Task<int> GetMembersCountAsync(
        OrganizationUnit organizationUnit,
        string? filter = null,
        bool includeChildren = false,
        CancellationToken cancellationToken = default)
    {
        return GetMembersCountAsync(organizationUnit.Id, filter, cancellationToken);
    }

    public virtual async Task<List<IdentityUser>> GetUnaddedUsersAsync(
        OrganizationUnit organizationUnit,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string? filter = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = dbContext.Users
            .IncludeDetails(includeDetails)
            .Where(u => !u.OrganizationUnits.Any(uou => uou.OrganizationUnitId == organizationUnit.Id));

        query = query.WhereIf(!filter.IsNullOrWhiteSpace(),
            u => u.UserName!.Contains(filter!) || u.Email.Contains(filter!));

        return await query
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(IdentityUser.UserName) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<int> GetUnaddedUsersCountAsync(
        OrganizationUnit organizationUnit,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = dbContext.Users
            .Where(u => !u.OrganizationUnits.Any(uou => uou.OrganizationUnitId == organizationUnit.Id));

        query = query.WhereIf(!filter.IsNullOrWhiteSpace(),
            u => u.UserName!.Contains(filter!) || u.Email.Contains(filter!));

        return await query.CountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual Task RemoveAllRolesAsync(
        OrganizationUnit organizationUnit,
        CancellationToken cancellationToken = default)
    {
        organizationUnit.Roles.Clear();
        return Task.CompletedTask;
    }

    public virtual async Task RemoveAllMembersAsync(
        OrganizationUnit organizationUnit,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var users = await dbContext.Users
            .Where(u => u.OrganizationUnits.Any(uou => uou.OrganizationUnitId == organizationUnit.Id))
            .ToListAsync(GetCancellationToken(cancellationToken));

        foreach (var user in users)
        {
            user.RemoveOrganizationUnit(organizationUnit.Id);
        }
    }

    public override async Task<IQueryable<OrganizationUnit>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}
