using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.Chat.ConversationLinks;

public interface IConversationLinkRepository : IRepository<ConversationLink, Guid>
{
    Task<List<ConversationLink>> GetListBySessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<List<ConversationLink>> GetListByEntityAsync(
        string linkedEntityType,
        string linkedEntityId,
        CancellationToken cancellationToken = default);
}
