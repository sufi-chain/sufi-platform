using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Channels;
using SufiChain.SufiPlatform.SufiCom.Channels.Metadata;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Sessions;

public class ChatSession_Tests
{
    [Fact]
    public void Should_Start_As_Open()
    {
        var session = new ChatSession(
            Guid.NewGuid(),
            null,
            "Title",
            AccessMode.PublicAuthenticated,
            ConversationKind.Direct,
            ChannelOrigin.Web);

        session.Status.ShouldBe(ChatSessionStatus.Open);
    }

    [Fact]
    public void Should_Throw_When_Sending_To_Closed_Session()
    {
        var session = new ChatSession(
            Guid.NewGuid(),
            null,
            null,
            AccessMode.PublicAuthenticated,
            ConversationKind.Direct,
            ChannelOrigin.Web);

        session.Close();

        var exception = Should.Throw<BusinessException>(() => session.EnsureOpen());
        exception.Code.ShouldBe(ChatErrorCodes.SessionClosed);
    }

    [Fact]
    public void Should_Store_Assistant_Context_In_Extra_Properties()
    {
        var workspaceId = Guid.NewGuid();
        var assistantId = Guid.NewGuid();
        var session = new ChatSession(
            Guid.NewGuid(),
            null,
            null,
            AccessMode.PublicAuthenticated,
            ConversationKind.Assistant,
            ChannelOrigin.Web);

        session.SetAssistantContext(workspaceId, "support", assistantId, externalOrchestration: true);

        session.GetAssistantWorkspaceId().ShouldBe(workspaceId);
        session.GetAssistantWorkspaceName().ShouldBe("support");
        session.GetAssistantId().ShouldBe(assistantId);
        session.IsExternallyOrchestrated().ShouldBeTrue();
        session.GetOrigin().ShouldBe(ChatSessionOrigin.Default);
    }

    [Fact]
    public void Should_Store_Playground_Origin()
    {
        var session = new ChatSession(
            Guid.NewGuid(),
            null,
            null,
            AccessMode.Internal,
            ConversationKind.Assistant,
            ChannelOrigin.Api);

        session.SetAssistantContext(
            workspaceId: null,
            workspaceName: null,
            assistantId: Guid.NewGuid(),
            externalOrchestration: true,
            origin: ChatSessionOrigin.HooshvarePlayground);

        session.GetOrigin().ShouldBe(ChatSessionOrigin.HooshvarePlayground);
        session.IsExternallyOrchestrated().ShouldBeTrue();
    }

    [Fact]
    public void Should_Store_Contextual_Origin()
    {
        var session = new ChatSession(
            Guid.NewGuid(),
            null,
            null,
            AccessMode.Internal,
            ConversationKind.Assistant,
            ChannelOrigin.Admin);

        session.SetAssistantContext(
            workspaceId: null,
            workspaceName: null,
            assistantId: Guid.NewGuid(),
            externalOrchestration: true,
            origin: ChatSessionOrigin.Contextual);

        session.GetOrigin().ShouldBe(ChatSessionOrigin.Contextual);
        session.IsExternallyOrchestrated().ShouldBeTrue();
    }

    [Fact]
    public void Should_Store_Connector_Metadata_In_Namespaced_Extra_Properties()
    {
        var session = new ChatSession(
            Guid.NewGuid(),
            null,
            null,
            AccessMode.PublicAnonymous,
            ConversationKind.Support,
            ChannelOrigin.Email,
            new ChannelSessionConnectorMetadata
            {
                ConnectorName = "Email",
                ExternalThreadId = "thread-001",
                LastExternalMessageId = "message-001",
                ExternalParticipantAddress = "visitor@example.com"
            });

        var metadata = session.GetConnectorMetadata();

        metadata.ShouldNotBeNull();
        metadata!.ConnectorName.ShouldBe("Email");
        metadata.ExternalThreadId.ShouldBe("thread-001");
        metadata.LastExternalMessageId.ShouldBe("message-001");
        metadata.ExternalParticipantAddress.ShouldBe("visitor@example.com");
    }

    [Fact]
    public void Should_Round_Trip_ConnectionId_In_Connector_Metadata()
    {
        var connectionId = Guid.NewGuid();
        var session = new ChatSession(
            Guid.NewGuid(),
            null,
            null,
            AccessMode.PublicAnonymous,
            ConversationKind.Support,
            ChannelOrigin.Api,
            new ChannelSessionConnectorMetadata
            {
                ConnectorName = ChannelNames.TelegramUser,
                ExternalThreadId = "tg-chat-123",
                ConnectionId = connectionId
            });

        var metadata = session.GetConnectorMetadata();

        metadata.ShouldNotBeNull();
        metadata!.ConnectionId.ShouldBe(connectionId);
    }

    [Fact]
    public void Should_Not_Store_ConnectionId_When_Null()
    {
        var session = new ChatSession(
            Guid.NewGuid(),
            null,
            null,
            AccessMode.PublicAnonymous,
            ConversationKind.Email,
            ChannelOrigin.Email,
            new ChannelSessionConnectorMetadata
            {
                ConnectorName = ChannelNames.Email,
                ExternalThreadId = "thread-001"
            });

        var metadata = session.GetConnectorMetadata();

        metadata.ShouldNotBeNull();
        metadata!.ConnectionId.ShouldBeNull();
    }
}
