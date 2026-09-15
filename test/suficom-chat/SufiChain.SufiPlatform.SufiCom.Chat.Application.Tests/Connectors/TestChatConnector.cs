using SufiChain.SufiPlatform.SufiCom.Channels.Outbound;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiCom.Channels;

public class TestChatConnector : IChannelConnector, ISingletonDependency
{
    public string Name => "Test";

    public ChannelOrigin ChannelOrigin => ChannelOrigin.Api;

    public ConversationKind DefaultConversationKind => ConversationKind.Support;

    public Task<DispatchOutboundChannelMessageResult> DispatchOutboundAsync(
        DispatchOutboundChannelMessageInput input,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new DispatchOutboundChannelMessageResult
        {
            Succeeded = true,
            ExternalMessageId = "test-outbound"
        });
    }
}
