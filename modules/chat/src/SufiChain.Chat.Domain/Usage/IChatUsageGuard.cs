using System;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.Chat.Usage;

public interface IChatUsageGuard
{
    Task<ChatUsageCheckResult> CheckCanStartSessionAsync(ChatStartSessionContext context, CancellationToken ct = default);

    Task<ChatUsageCheckResult> CheckCanSendMessageAsync(Guid sessionId, ChatMessageSenderKind sender, CancellationToken ct = default);

    Task<ChatUsageCheckResult> CheckCanSendMessageAsync(ChatSendMessageContext context, CancellationToken ct = default);

    Task<ChatUsageCheckResult> CheckCanAttachFileAsync(
        Guid sessionId,
        int additionalAttachmentCount,
        long additionalBytes,
        AccessMode accessMode,
        CancellationToken ct = default);

    Task RecordMessageSentAsync(
        Guid sessionId,
        ChatMessageSenderKind sender,
        int attachmentCount = 0,
        long attachmentBytes = 0,
        CancellationToken ct = default);

    Task<ChatUsageCheckResult> CheckCanEnterAiHandoffAsync(Guid sessionId, ChatAiOperationContext context, CancellationToken ct = default);

    Task<ChatUsageCheckResult> CheckCanInvokeAiAsync(Guid sessionId, ChatAiOperationKind operation, CancellationToken ct = default);

    Task<Guid> ReserveAiUsageAsync(Guid sessionId, ChatAiOperationKind operation, CancellationToken ct = default);

    Task RecordAiUsageAsync(Guid reservationId, ChatAiUsageRecord record, CancellationToken ct = default);

    Task ReleaseAiReservationAsync(Guid reservationId, CancellationToken ct = default);
}
