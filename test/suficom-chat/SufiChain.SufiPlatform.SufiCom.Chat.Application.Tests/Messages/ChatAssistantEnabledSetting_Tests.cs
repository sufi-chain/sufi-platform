using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Chat.Application.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.ETOs;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using SufiChain.SufiPlatform.SufiCom.Chat.Supports;
using SufiChain.SufiPlatform.SufiCom.Chat.Usage;
using Shouldly;
using SufiChain.SufiPlatform.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Messages;

/// <summary>
/// sufi-chain#571: Chat.Ai.Enabled is enforced on the server. When it is off, an assistant session
/// gets no reply and no usage row, including an internal session on a tenant.
/// </summary>
public class ChatAssistantEnabledSetting_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatSessionAppService _sessionAppService;
    private readonly IChatMessageRepository _messageRepository;
    private readonly IChatAiUsageReservationRepository _reservations;
    private readonly ISettingManager _settingManager;
    private readonly ChatAssistantMessageHandler _handler;
    private readonly ConfigurableAiService _ai;

    public ChatAssistantEnabledSetting_Tests()
    {
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _messageRepository = GetRequiredService<IChatMessageRepository>();
        _reservations = GetRequiredService<IChatAiUsageReservationRepository>();
        _settingManager = GetRequiredService<ISettingManager>();
        _handler = GetRequiredService<ChatAssistantMessageHandler>();
        _ai = GetRequiredService<ConfigurableAiService>();
    }

    [Fact]
    public async Task When_ai_is_off_an_internal_session_gets_no_reply_and_no_usage()
    {
        var called = 0;
        var previous = _ai.BeforeComplete;
        var previousContent = _ai.ResponseContent;
        _ai.ResponseContent = "should not be stored";
        _ai.BeforeComplete = _ =>
        {
            called++;
            return Task.CompletedTask;
        };
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: false);
            var session = await CreateAssistantSessionAsync();
            var userMessage = await InsertVisitorMessageAsync(session.Id, "hello");

            await _handler.ProcessAsync(EventFor(session.Id, userMessage.Id, tenantId: null));

            called.ShouldBe(0);
            var messages = await _messageRepository.GetListBySessionAsync(session.Id, includeInternal: false);
            messages.ShouldNotContain(message => message.SenderKind == ChatMessageSenderKind.Assistant);
            (await _reservations.GetListAsync()).ShouldNotContain(row => row.SessionId == session.Id);
        }
        finally
        {
            _ai.BeforeComplete = previous;
            _ai.ResponseContent = previousContent;
        }
    }

    [Fact]
    public async Task When_ai_is_on_the_reply_is_stored_and_usage_is_recorded()
    {
        var previousContent = _ai.ResponseContent;
        _ai.ResponseContent = "enabled reply";
        _ai.BeforeComplete = null;
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: true);
            var session = await CreateAssistantSessionAsync();
            var userMessage = await InsertVisitorMessageAsync(session.Id, "hello");

            await _handler.ProcessAsync(EventFor(session.Id, userMessage.Id, tenantId: null));

            var messages = await _messageRepository.GetListBySessionAsync(session.Id, includeInternal: false);
            messages.ShouldContain(message =>
                message.SenderKind == ChatMessageSenderKind.Assistant && message.Body == "enabled reply");
            (await _reservations.GetListAsync()).ShouldContain(row => row.SessionId == session.Id);
        }
        finally
        {
            _ai.ResponseContent = previousContent;
        }
    }

    [Fact]
    public async Task A_tenant_with_ai_off_gets_no_reply_even_when_the_host_setting_is_on()
    {
        var called = 0;
        var previous = _ai.BeforeComplete;
        _ai.BeforeComplete = _ =>
        {
            called++;
            return Task.CompletedTask;
        };
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: true);
            await _settingManager.SetForTenantAsync(ChatTestData.TenantAId, ChatSettingNames.Ai.Enabled, false.ToString());
            ChatSessionDto session;
            ChatMessage userMessage;
            using (CurrentTenant.Change(ChatTestData.TenantAId))
            {
                session = await CreateAssistantSessionAsync();
                userMessage = await InsertVisitorMessageAsync(session.Id, "hello");
            }

            await _handler.ProcessAsync(EventFor(session.Id, userMessage.Id, ChatTestData.TenantAId));

            called.ShouldBe(0);
            using (CurrentTenant.Change(ChatTestData.TenantAId))
            {
                var messages = await _messageRepository.GetListBySessionAsync(session.Id, includeInternal: false);
                messages.ShouldNotContain(message => message.SenderKind == ChatMessageSenderKind.Assistant);
                (await _reservations.GetListAsync()).ShouldNotContain(row => row.SessionId == session.Id);
            }
        }
        finally
        {
            _ai.BeforeComplete = previous;
            await _settingManager.SetForTenantAsync(ChatTestData.TenantAId, ChatSettingNames.Ai.Enabled, null);
        }
    }

    private static ChatMessageSentEto EventFor(Guid sessionId, Guid messageId, Guid? tenantId) => new()
    {
        Id = Guid.Empty,
        TenantId = tenantId,
        SessionId = sessionId,
        MessageId = messageId,
        SenderKind = ChatMessageSenderKind.Visitor,
        SentAt = DateTime.UtcNow
    };

    private async Task<ChatSessionDto> CreateAssistantSessionAsync()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            return await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                Title = "Ai enabled",
                AccessMode = AccessMode.Internal,
                ConversationKind = ConversationKind.Assistant,
                ChannelOrigin = ChannelOrigin.Web,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });
        }
    }

    private async Task<ChatMessage> InsertVisitorMessageAsync(Guid sessionId, string body)
    {
        var message = new ChatMessage(
            Guid.NewGuid(),
            CurrentTenant.Id,
            sessionId,
            body,
            ChatMessageSenderKind.Visitor);
        return await _messageRepository.InsertAsync(message, autoSave: true);
    }
}
