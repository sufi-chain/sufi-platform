using SufiChain.Chat.Connectors.Outbound;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Connectors;

public class TestChatConnector : IChatConnector, ISingletonDependency
{
    public string Name => "Test";

    public ChannelOrigin ChannelOrigin => ChannelOrigin.Api;

    public ConversationKind DefaultConversationKind => ConversationKind.Support;

    public Task<DispatchOutboundChatMessageResult> DispatchOutboundAsync(
        DispatchOutboundChatMessageInput input,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new DispatchOutboundChatMessageResult
        {
            Succeeded = true,
            ExternalMessageId = "test-outbound"
        });
    }
}
