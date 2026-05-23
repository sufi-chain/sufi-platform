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

public class EfCoreIdentitySessionRepository : EfCoreRepository<ISufiAbpIdentityDbContext, IdentitySession, Guid>, IIdentitySessionRepository
{
    public EfCoreIdentitySessionRepository(IDbContextProvider<ISufiAbpIdentityDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<IdentitySession?> FindBySessionIdAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .FirstOrDefaultAsync(
                s => s.SessionId == sessionId,
                GetCancellationToken(cancellationToken)
            );
    }

    public virtual async Task<List<IdentitySession>> GetListAsync(
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        Guid? userId = null,
        string? device = null,
        string? clientId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .WhereIf(userId.HasValue, s => s.UserId == userId!.Value)
            .WhereIf(!device.IsNullOrWhiteSpace(), s => s.Device == device)
            .WhereIf(!clientId.IsNullOrWhiteSpace(), s => s.ClientId == clientId)
            .OrderBy(sorting.IsNullOrWhiteSpace() ? $"{nameof(IdentitySession.LastAccessed)} DESC" : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<long> GetCountAsync(
        Guid? userId = null,
        string? device = null,
        string? clientId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .WhereIf(userId.HasValue, s => s.UserId == userId!.Value)
            .WhereIf(!device.IsNullOrWhiteSpace(), s => s.Device == device)
            .WhereIf(!clientId.IsNullOrWhiteSpace(), s => s.ClientId == clientId)
            .LongCountAsync(GetCancellationToken(cancellationToken));
    }
}
