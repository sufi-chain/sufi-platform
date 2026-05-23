using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.Identity;

public interface IIdentitySessionRepository : IBasicRepository<IdentitySession, Guid>
{
    Task<IdentitySession?> FindBySessionIdAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Task<List<IdentitySession>> GetListAsync(
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        Guid? userId = null,
        string? device = null,
        string? clientId = null,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        Guid? userId = null,
        string? device = null,
        string? clientId = null,
        CancellationToken cancellationToken = default);
}
