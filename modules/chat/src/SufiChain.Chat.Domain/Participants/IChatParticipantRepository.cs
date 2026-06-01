using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.Chat.Participants;

public interface IChatParticipantRepository : IRepository<ChatParticipant, Guid>
{
    Task<List<ChatParticipant>> GetListBySessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<bool> IsParticipantAsync(
        Guid sessionId,
        Guid? userId = null,
        string? anonymousVisitorId = null,
        CancellationToken cancellationToken = default);

    Task<int> GetActiveCountAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
