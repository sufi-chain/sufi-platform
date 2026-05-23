using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.Identity;

public interface IIdentityLinkUserRepository : IBasicRepository<IdentityLinkUser, Guid>
{
    Task<IdentityLinkUser?> FindAsync(
        Guid sourceUserId,
        Guid? sourceTenantId,
        Guid targetUserId,
        Guid? targetTenantId,
        CancellationToken cancellationToken = default);

    Task<List<IdentityLinkUser>> GetListAsync(
        Guid sourceUserId,
        Guid? sourceTenantId,
        CancellationToken cancellationToken = default);
}
