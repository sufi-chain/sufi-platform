using SufiChain.Chat.Connectors.Inbound;
using Volo.Abp.Application.Services;

namespace SufiChain.Chat.Connectors;

/// <summary>
/// Ingests inbound messages from channel connectors into Chat sessions and messages.
/// </summary>
public interface IChatInboundMessageAppService : IApplicationService
{
    Task<IngestInboundChatMessageResult> IngestAsync(IngestInboundChatMessageInput input);
}
