using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.Identity;

public interface IIdentityClaimTypeRepository : IBasicRepository<IdentityClaimType, Guid>
{
    Task<bool> AnyAsync(
        string name,
        Guid? ignoredId = null,
        CancellationToken cancellationToken = default);

    Task<List<IdentityClaimType>> GetListAsync(
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string? filter = null,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? filter = null,
        CancellationToken cancellationToken = default);
}
