using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.Identity;

public interface IIdentityUserDelegationRepository : IBasicRepository<IdentityUserDelegation, Guid>
{
    Task<List<IdentityUserDelegation>> GetListAsync(
        Guid? sourceUserId = null,
        Guid? targetUserId = null,
        CancellationToken cancellationToken = default);
}
