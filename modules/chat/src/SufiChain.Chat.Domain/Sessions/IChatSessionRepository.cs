using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.Chat.Sessions;

public interface IChatSessionRepository : IRepository<ChatSession, Guid>
{
    Task<ChatSession?> FindDirectSessionByUserPairAsync(
        Guid? tenantId,
        Guid userId,
        Guid otherUserId,
        CancellationToken cancellationToken = default);

    Task<List<ChatSession>> GetSessionsForParticipantAsync(
        Guid? tenantId,
        Guid? userId = null,
        string? anonymousVisitorId = null,
        int skipCount = 0,
        int maxResultCount = 10,
        CancellationToken cancellationToken = default);

    Task<ChatSession?> FindByConnectorExternalThreadAsync(
        Guid? tenantId,
        string connectorName,
        string externalThreadId,
        CancellationToken cancellationToken = default);
}
