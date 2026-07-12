using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Identity.EntityFrameworkCore;

public class EfCoreIdentitySecurityLogRepository : EfCoreRepository<ISufiIdentityDbContext, IdentitySecurityLog, Guid>, IIdentitySecurityLogRepository
{
    public EfCoreIdentitySecurityLogRepository(IDbContextProvider<ISufiIdentityDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<List<IdentitySecurityLog>> GetListAsync(
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        DateTime? startTime = null,
        DateTime? endTime = null,
        string? applicationName = null,
        string? identity = null,
        string? action = null,
        Guid? userId = null,
        string? userName = null,
        string? clientId = null,
        string? correlationId = null,
        string? clientIpAddress = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .WhereIf(startTime.HasValue, log => log.CreationTime >= startTime!.Value)
            .WhereIf(endTime.HasValue, log => log.CreationTime <= endTime!.Value)
            .WhereIf(!applicationName.IsNullOrWhiteSpace(), log => log.ApplicationName == applicationName)
            .WhereIf(!identity.IsNullOrWhiteSpace(), log => log.Identity == identity)
            .WhereIf(!action.IsNullOrWhiteSpace(), log => log.Action == action)
            .WhereIf(userId.HasValue, log => log.UserId == userId)
            .WhereIf(!userName.IsNullOrWhiteSpace(), log => log.UserName == userName)
            .WhereIf(!clientId.IsNullOrWhiteSpace(), log => log.ClientId == clientId)
            .WhereIf(!correlationId.IsNullOrWhiteSpace(), log => log.CorrelationId == correlationId)
            .WhereIf(!clientIpAddress.IsNullOrWhiteSpace(), log => log.ClientIpAddress == clientIpAddress)
            .OrderBy(sorting.IsNullOrWhiteSpace() ? $"{nameof(IdentitySecurityLog.CreationTime)} DESC" : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<long> GetCountAsync(
        DateTime? startTime = null,
        DateTime? endTime = null,
        string? applicationName = null,
        string? identity = null,
        string? action = null,
        Guid? userId = null,
        string? userName = null,
        string? clientId = null,
        string? correlationId = null,
        string? clientIpAddress = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .WhereIf(startTime.HasValue, log => log.CreationTime >= startTime!.Value)
            .WhereIf(endTime.HasValue, log => log.CreationTime <= endTime!.Value)
            .WhereIf(!applicationName.IsNullOrWhiteSpace(), log => log.ApplicationName == applicationName)
            .WhereIf(!identity.IsNullOrWhiteSpace(), log => log.Identity == identity)
            .WhereIf(!action.IsNullOrWhiteSpace(), log => log.Action == action)
            .WhereIf(userId.HasValue, log => log.UserId == userId)
            .WhereIf(!userName.IsNullOrWhiteSpace(), log => log.UserName == userName)
            .WhereIf(!clientId.IsNullOrWhiteSpace(), log => log.ClientId == clientId)
            .WhereIf(!correlationId.IsNullOrWhiteSpace(), log => log.CorrelationId == correlationId)
            .WhereIf(!clientIpAddress.IsNullOrWhiteSpace(), log => log.ClientIpAddress == clientIpAddress)
            .LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<IdentitySecurityLog?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.CreationTime)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }
}
