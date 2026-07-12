using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Identity.EntityFrameworkCore;

public class EfCoreIdentityUserRepository : EfCoreRepository<ISufiIdentityDbContext, IdentityUser, Guid>, IIdentityUserRepository
{
    public EfCoreIdentityUserRepository(IDbContextProvider<ISufiIdentityDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<IdentityUser?> FindByNormalizedUserNameAsync(
        string normalizedUserName,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(
                u => u.NormalizedUserName == normalizedUserName,
                GetCancellationToken(cancellationToken)
            );
    }

    public virtual async Task<List<string>> GetRoleNamesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from userRole in dbContext.Set<IdentityUserRole>()
                    join role in dbContext.Roles on userRole.RoleId equals role.Id
                    where userRole.UserId == id
                    select role.Name;

        var organizationUnitIds = dbContext.Set<IdentityUserOrganizationUnit>()
            .Where(q => q.UserId == id)
            .Select(q => q.OrganizationUnitId)
            .ToArray();

        var organizationRoleIds = await (
            from ouRole in dbContext.Set<OrganizationUnitRole>()
            where organizationUnitIds.Contains(ouRole.OrganizationUnitId)
            select ouRole.RoleId
        ).ToListAsync(GetCancellationToken(cancellationToken));

        var orgUnitRoleNameQuery = dbContext.Roles
            .Where(r => organizationRoleIds.Contains(r.Id))
            .Select(n => n.Name);

        var resultQuery = query.Union(orgUnitRoleNameQuery);
        return await resultQuery.ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<string>> GetRoleNamesInOrganizationUnitAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var organizationUnitIds = dbContext.Set<IdentityUserOrganizationUnit>()
            .Where(q => q.UserId == id)
            .Select(q => q.OrganizationUnitId)
            .ToArray();

        var organizationRoleIds = await (
            from ouRole in dbContext.Set<OrganizationUnitRole>()
            where organizationUnitIds.Contains(ouRole.OrganizationUnitId)
            select ouRole.RoleId
        ).ToListAsync(GetCancellationToken(cancellationToken));

        return await dbContext.Roles
            .Where(r => organizationRoleIds.Contains(r.Id))
            .Select(n => n.Name)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<IdentityUser?> FindByLoginAsync(
        string loginProvider,
        string providerKey,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from user in dbContext.Users.IncludeDetails(includeDetails)
                    join userLogin in dbContext.Set<IdentityUserLogin>() on user.Id equals userLogin.UserId
                    where userLogin.LoginProvider == loginProvider && userLogin.ProviderKey == providerKey
                    select user;

        return await query.FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<IdentityUser?> FindByNormalizedEmailAsync(
        string normalizedEmail,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(
                u => u.NormalizedEmail == normalizedEmail,
                GetCancellationToken(cancellationToken)
            );
    }

    public virtual async Task<List<IdentityUser>> GetListByClaimAsync(
        Claim claim,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from user in dbContext.Users.IncludeDetails(includeDetails)
                    join userClaim in dbContext.Set<IdentityUserClaim>() on user.Id equals userClaim.UserId
                    where userClaim.ClaimType == claim.Type && userClaim.ClaimValue == claim.Value
                    select user;

        return await query.ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityUser>> GetListByNormalizedRoleNameAsync(
        string normalizedRoleName,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var role = await dbContext.Roles
            .Where(x => x.NormalizedName == normalizedRoleName)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));

        if (role == null)
        {
            return new List<IdentityUser>();
        }

        var query = from user in dbContext.Users.IncludeDetails(includeDetails)
                    join userRole in dbContext.Set<IdentityUserRole>() on user.Id equals userRole.UserId
                    where userRole.RoleId == role.Id
                    select user;

        return await query.ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<Guid>> GetUserIdListByRoleIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Set<IdentityUserRole>()
            .Where(userRole => userRole.RoleId == roleId)
            .Select(userRole => userRole.UserId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityRole>> GetRolesAsync(
        Guid id,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from userRole in dbContext.Set<IdentityUserRole>()
                    join role in dbContext.Roles.IncludeDetails(includeDetails) on userRole.RoleId equals role.Id
                    where userRole.UserId == id
                    select role;

        return await query.ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<OrganizationUnit>> GetOrganizationUnitsAsync(
        Guid id,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from userOU in dbContext.Set<IdentityUserOrganizationUnit>()
                    join ou in dbContext.OrganizationUnits.IncludeDetails(includeDetails) on userOU.OrganizationUnitId equals ou.Id
                    where userOU.UserId == id
                    select ou;

        return await query.ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityUser>> GetUsersInOrganizationUnitAsync(
        Guid organizationUnitId,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from userOU in dbContext.Set<IdentityUserOrganizationUnit>()
                    join user in dbContext.Users on userOU.UserId equals user.Id
                    where userOU.OrganizationUnitId == organizationUnitId
                    select user;

        return await query.ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityUser>> GetUsersInOrganizationsListAsync(
        List<Guid> organizationUnitIds,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from userOU in dbContext.Set<IdentityUserOrganizationUnit>()
                    join user in dbContext.Users on userOU.UserId equals user.Id
                    where organizationUnitIds.Contains(userOU.OrganizationUnitId)
                    select user;

        return await query.ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityUser>> GetUsersInOrganizationUnitWithChildrenAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var query = from userOU in dbContext.Set<IdentityUserOrganizationUnit>()
                    join user in dbContext.Users on userOU.UserId equals user.Id
                    join ou in dbContext.OrganizationUnits on userOU.OrganizationUnitId equals ou.Id
                    where ou.Code.StartsWith(code)
                    select user;

        return await query.ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<IdentityUser?> FindByTenantIdAndUserNameAsync(
        string userName,
        Guid? tenantId,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(
                u => u.TenantId == tenantId && u.UserName == userName,
                GetCancellationToken(cancellationToken)
            );
    }

    public virtual async Task<List<IdentityUser>> GetListByIdsAsync(
        IEnumerable<Guid> ids,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityUser>> GetListAsync(
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string? filter = null,
        bool includeDetails = false,
        Guid? roleId = null,
        Guid? organizationUnitId = null,
        string? userName = null,
        string? phoneNumber = null,
        string? emailAddress = null,
        string? name = null,
        string? surname = null,
        bool? isLockedOut = null,
        bool? notActive = null,
        bool? emailConfirmed = null,
        bool? isExternal = null,
        DateTime? maxCreationTime = null,
        DateTime? minCreationTime = null,
        DateTime? maxModificationTime = null,
        DateTime? minModificationTime = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetListQueryAsync(
            filter,
            includeDetails,
            roleId,
            organizationUnitId,
            userName,
            phoneNumber,
            emailAddress,
            name,
            surname,
            isLockedOut,
            notActive,
            emailConfirmed,
            isExternal,
            maxCreationTime,
            minCreationTime,
            maxModificationTime,
            minModificationTime
        );

        return await query
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(IdentityUser.UserName) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<long> GetCountAsync(
        string? filter = null,
        Guid? roleId = null,
        Guid? organizationUnitId = null,
        string? userName = null,
        string? phoneNumber = null,
        string? emailAddress = null,
        string? name = null,
        string? surname = null,
        bool? isLockedOut = null,
        bool? notActive = null,
        bool? emailConfirmed = null,
        bool? isExternal = null,
        DateTime? maxCreationTime = null,
        DateTime? minCreationTime = null,
        DateTime? maxModificationTime = null,
        DateTime? minModificationTime = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetListQueryAsync(
            filter,
            false,
            roleId,
            organizationUnitId,
            userName,
            phoneNumber,
            emailAddress,
            name,
            surname,
            isLockedOut,
            notActive,
            emailConfirmed,
            isExternal,
            maxCreationTime,
            minCreationTime,
            maxModificationTime,
            minModificationTime
        );

        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    protected virtual async Task<IQueryable<IdentityUser>> GetListQueryAsync(
        string? filter = null,
        bool includeDetails = false,
        Guid? roleId = null,
        Guid? organizationUnitId = null,
        string? userName = null,
        string? phoneNumber = null,
        string? emailAddress = null,
        string? name = null,
        string? surname = null,
        bool? isLockedOut = null,
        bool? notActive = null,
        bool? emailConfirmed = null,
        bool? isExternal = null,
        DateTime? maxCreationTime = null,
        DateTime? minCreationTime = null,
        DateTime? maxModificationTime = null,
        DateTime? minModificationTime = null)
    {
        var dbContext = await GetDbContextAsync();
        var query = (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .WhereIf(
                !filter.IsNullOrWhiteSpace(),
                u =>
                    u.UserName!.Contains(filter!) ||
                    u.Email.Contains(filter!) ||
                    (u.Name != null && u.Name.Contains(filter!)) ||
                    (u.Surname != null && u.Surname.Contains(filter!)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(filter!))
            )
            .WhereIf(!userName.IsNullOrWhiteSpace(), u => u.UserName!.Contains(userName!))
            .WhereIf(!emailAddress.IsNullOrWhiteSpace(), u => u.Email.Contains(emailAddress!))
            .WhereIf(!phoneNumber.IsNullOrWhiteSpace(), u => u.PhoneNumber != null && u.PhoneNumber.Contains(phoneNumber!))
            .WhereIf(!name.IsNullOrWhiteSpace(), u => u.Name != null && u.Name.Contains(name!))
            .WhereIf(!surname.IsNullOrWhiteSpace(), u => u.Surname != null && u.Surname.Contains(surname!))
            .WhereIf(isLockedOut.HasValue, u => u.LockoutEnabled && u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow)
            .WhereIf(notActive.HasValue, u => !u.IsActive)
            .WhereIf(emailConfirmed.HasValue, u => u.EmailConfirmed == emailConfirmed!.Value)
            .WhereIf(isExternal.HasValue, u => u.IsExternal == isExternal!.Value)
            .WhereIf(maxCreationTime.HasValue, u => u.CreationTime <= maxCreationTime!.Value)
            .WhereIf(minCreationTime.HasValue, u => u.CreationTime >= minCreationTime!.Value)
            .WhereIf(maxModificationTime.HasValue, u => u.LastModificationTime <= maxModificationTime!.Value)
            .WhereIf(minModificationTime.HasValue, u => u.LastModificationTime >= minModificationTime!.Value);

        if (roleId.HasValue)
        {
            var userIdsInRole = dbContext.Set<IdentityUserRole>()
                .Where(userRole => userRole.RoleId == roleId.Value)
                .Select(userRole => userRole.UserId);
            query = query.Where(u => userIdsInRole.Contains(u.Id));
        }

        if (organizationUnitId.HasValue)
        {
            var userIdsInOrganizationUnit = dbContext.Set<IdentityUserOrganizationUnit>()
                .Where(userOU => userOU.OrganizationUnitId == organizationUnitId.Value)
                .Select(userOU => userOU.UserId);
            query = query.Where(u => userIdsInOrganizationUnit.Contains(u.Id));
        }

        return query;
    }

    public override async Task<IQueryable<IdentityUser>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}
