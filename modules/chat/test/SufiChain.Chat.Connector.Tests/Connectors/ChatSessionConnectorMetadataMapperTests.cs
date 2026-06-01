using SufiChain.Chat.Connectors.Metadata;
using Shouldly;
using Xunit;

namespace SufiChain.Chat.Connectors;

public class ChatSessionConnectorMetadataMapperTests
{
    [Fact]
    public void Should_round_trip_session_connector_metadata()
    {
        var metadata = new ChatSessionConnectorMetadata
        {
            ConnectorName = ChatConnectorNames.Email,
            ExternalThreadId = "thread-abc",
            LastExternalMessageId = "msg-001",
            InReplyToExternalMessageId = "msg-root"
        };

        var json = ChatSessionConnectorMetadataMapper.BuildSessionMetadata(metadata);
        var read = ChatSessionConnectorMetadataMapper.TryReadSessionMetadata(json);

        read.ShouldNotBeNull();
        read!.ConnectorName.ShouldBe(ChatConnectorNames.Email);
        read.ExternalThreadId.ShouldBe("thread-abc");
        read.LastExternalMessageId.ShouldBe("msg-001");
        read.InReplyToExternalMessageId.ShouldBe("msg-root");
    }

    [Fact]
    public void Should_build_stable_lookup_token()
    {
        var token = ChatSessionConnectorMetadataMapper.BuildLookupToken("Email", "thread-abc");
        var json = ChatSessionConnectorMetadataMapper.BuildSessionMetadata(new ChatSessionConnectorMetadata
        {
            ConnectorName = "Email",
            ExternalThreadId = "thread-abc"
        });

        json.ShouldContain(token);
    }
}
