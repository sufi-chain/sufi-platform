using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.Chat.Usage;

public interface IChatAiUsageReservationRepository : IRepository<ChatAiUsageReservation, Guid>
{
    Task<long> GetSessionAiReplyCountAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<long> GetSessionTokenCountAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<long> GetTenantTokenCountAsync(
        Guid? tenantId,
        DateTime dayStart,
        CancellationToken cancellationToken = default);

    Task<long> GetOperatorOperationCountAsync(
        Guid operatorUserId,
        ChatAiOperationKind operationKind,
        DateTime dayStart,
        CancellationToken cancellationToken = default);
}
