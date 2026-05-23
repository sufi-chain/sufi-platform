using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.Identity.EntityFrameworkCore;

public class EfCoreIdentityUserDelegationRepository : EfCoreRepository<ISufiAbpIdentityDbContext, IdentityUserDelegation, Guid>, IIdentityUserDelegationRepository
{
    public EfCoreIdentityUserDelegationRepository(IDbContextProvider<ISufiAbpIdentityDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<List<IdentityUserDelegation>> GetListAsync(
        Guid? sourceUserId = null,
        Guid? targetUserId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .WhereIf(sourceUserId.HasValue, d => d.SourceUserId == sourceUserId!.Value)
            .WhereIf(targetUserId.HasValue, d => d.TargetUserId == targetUserId!.Value)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }
}
