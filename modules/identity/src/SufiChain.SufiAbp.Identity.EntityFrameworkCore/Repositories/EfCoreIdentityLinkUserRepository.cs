using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.Identity.EntityFrameworkCore;

public class EfCoreIdentityLinkUserRepository : EfCoreRepository<ISufiAbpIdentityDbContext, IdentityLinkUser, Guid>, IIdentityLinkUserRepository
{
    public EfCoreIdentityLinkUserRepository(IDbContextProvider<ISufiAbpIdentityDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<IdentityLinkUser?> FindAsync(
        Guid sourceUserId,
        Guid? sourceTenantId,
        Guid targetUserId,
        Guid? targetTenantId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .FirstOrDefaultAsync(
                link =>
                    link.SourceUserId == sourceUserId &&
                    link.SourceTenantId == sourceTenantId &&
                    link.TargetUserId == targetUserId &&
                    link.TargetTenantId == targetTenantId,
                GetCancellationToken(cancellationToken)
            );
    }

    public virtual async Task<List<IdentityLinkUser>> GetListAsync(
        Guid sourceUserId,
        Guid? sourceTenantId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(link => link.SourceUserId == sourceUserId && link.SourceTenantId == sourceTenantId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }
}
