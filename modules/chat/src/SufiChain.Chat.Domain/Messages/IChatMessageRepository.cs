using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.Chat.Messages;

public interface IChatMessageRepository : IRepository<ChatMessage, Guid>
{
    Task<List<ChatMessage>> GetListBySessionAsync(
        Guid sessionId,
        bool includeInternal = false,
        int skipCount = 0,
        int maxResultCount = 50,
        CancellationToken cancellationToken = default);

    Task<long> GetCountBySessionAsync(
        Guid sessionId,
        bool includeInternal = false,
        CancellationToken cancellationToken = default);
}
