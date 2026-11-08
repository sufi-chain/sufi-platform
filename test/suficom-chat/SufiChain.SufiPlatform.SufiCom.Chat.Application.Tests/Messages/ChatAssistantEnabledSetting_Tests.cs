using NSubstitute;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Chat.Application.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.ETOs;
using SufiChain.SufiPlatform.SufiCom.Chat.Features;
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

    [Fact]
    public async Task A_turn_queued_while_ai_is_on_does_not_run_after_the_setting_is_turned_off()
    {
        var called = 0;
        var previous = _ai.BeforeComplete;
        _ai.BeforeComplete = _ =>
        {
            called++;
            return Task.CompletedTask;
        };
        var wallets = GetRequiredService<TestChatUsageWalletResolver>();
        var previousWallet = wallets.Context;
        wallets.Context = new ChatUsageWalletContext
        {
            WalletId = Guid.NewGuid(),
            IsChargeRequired = true,
            Currency = "USD"
        };
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: true);
            var session = await CreateAssistantSessionAsync();
            var userMessage = await InsertVisitorMessageAsync(session.Id, "hello");
            await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: false);

            await _handler.ProcessAsync(EventFor(session.Id, userMessage.Id, tenantId: null));

            called.ShouldBe(0);
            var messages = await _messageRepository.GetListBySessionAsync(session.Id, includeInternal: false);
            messages.ShouldNotContain(message => message.SenderKind == ChatMessageSenderKind.Assistant);
            (await _reservations.GetListAsync()).ShouldNotContain(row =>
                row.SessionId == session.Id && row.IsWalletChargeRequired);
            (await _reservations.GetListAsync()).ShouldNotContain(row => row.SessionId == session.Id);
        }
        finally
        {
            _ai.BeforeComplete = previous;
            wallets.Context = previousWallet;
        }
    }

    [Fact]
    public async Task The_ai_feature_blocks_a_reply_even_when_the_setting_is_on()
    {
        var features = GetRequiredService<ConfigurableFeatureChecker>();
        var called = 0;
        var previous = _ai.BeforeComplete;
        _ai.BeforeComplete = _ =>
        {
            called++;
            return Task.CompletedTask;
        };
        features.Disable(ChatFeatures.Ai.Enable);
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: true);
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
            features.Enable(ChatFeatures.Ai.Enable);
            _ai.BeforeComplete = previous;
        }
    }

    [Fact]
    public async Task Another_tenant_still_receives_a_mocked_reply_when_a_different_tenant_has_ai_off()
    {
        var previousContent = _ai.ResponseContent;
        _ai.ResponseContent = "tenant b reply";
        _ai.BeforeComplete = null;
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: true);
            await _settingManager.SetForTenantAsync(ChatTestData.TenantAId, ChatSettingNames.Ai.Enabled, false.ToString());
            await _settingManager.SetForTenantAsync(ChatTestData.TenantBId, ChatSettingNames.Ai.Enabled, true.ToString());
            ChatSessionDto session;
            ChatMessage userMessage;
            using (CurrentTenant.Change(ChatTestData.TenantBId))
            {
                session = await CreateAssistantSessionAsync();
                userMessage = await InsertVisitorMessageAsync(session.Id, "hello");
            }

            await _handler.ProcessAsync(EventFor(session.Id, userMessage.Id, ChatTestData.TenantBId));

            using (CurrentTenant.Change(ChatTestData.TenantBId))
            {
                var messages = await _messageRepository.GetListBySessionAsync(session.Id, includeInternal: false);
                messages.ShouldContain(message =>
                    message.SenderKind == ChatMessageSenderKind.Assistant && message.Body == "tenant b reply");
                (await _reservations.GetListAsync()).ShouldContain(row => row.SessionId == session.Id);
            }

            using (CurrentTenant.Change(ChatTestData.TenantAId))
            {
                (await _reservations.GetListAsync()).ShouldNotContain(row => row.SessionId == session.Id);
            }
        }
        finally
        {
            _ai.ResponseContent = previousContent;
            await _settingManager.SetForTenantAsync(ChatTestData.TenantAId, ChatSettingNames.Ai.Enabled, null);
            await _settingManager.SetForTenantAsync(ChatTestData.TenantBId, ChatSettingNames.Ai.Enabled, null);
        }
    }

    [Fact]
    public async Task When_ai_is_off_an_audio_attachment_does_not_reach_a_provider()
    {
        var storage = GetRequiredService<IFileStorageIntegrationService>();
        var audio = GetRequiredService<ISufiAIAudioService>();
        storage.ClearReceivedCalls();
        audio.ClearReceivedCalls();
        var called = 0;
        var previous = _ai.BeforeComplete;
        _ai.BeforeComplete = _ =>
        {
            called++;
            return Task.CompletedTask;
        };
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: false);
            var session = await CreateAssistantSessionAsync();
            var fileId = Guid.NewGuid();
            var bytes = new byte[] { 1, 2, 3, 4 };
            storage.GetAsync(fileId).Returns(new FileReferenceDto
            {
                Id = fileId,
                MimeType = "audio/webm",
                FileName = "voice.webm",
                SizeInBytes = bytes.Length,
                EntityType = ChatEntityTypes.Session,
                EntityId = session.Id
            });
            storage.GetContentAsync(fileId).Returns(new FileContentBytesDto
            {
                Id = fileId,
                MimeType = "audio/webm",
                Content = bytes
            });
            var userMessage = await InsertVisitorMessageAsync(session.Id, "hello", [fileId]);

            await _handler.ProcessAsync(EventFor(session.Id, userMessage.Id, tenantId: null));

            called.ShouldBe(0);
            await storage.DidNotReceive().GetAsync(fileId);
            await audio.DidNotReceiveWithAnyArgs().TranscribeAsync(default!, default);
            var messages = await _messageRepository.GetListBySessionAsync(session.Id, includeInternal: false);
            messages.ShouldNotContain(message => message.SenderKind == ChatMessageSenderKind.Assistant);
            (await _reservations.GetListAsync()).ShouldNotContain(row => row.SessionId == session.Id);
        }
        finally
        {
            _ai.BeforeComplete = previous;
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

    private async Task<ChatMessage> InsertVisitorMessageAsync(
        Guid sessionId,
        string body,
        IEnumerable<Guid>? attachmentFileIds = null)
    {
        var message = new ChatMessage(
            Guid.NewGuid(),
            CurrentTenant.Id,
            sessionId,
            body,
            ChatMessageSenderKind.Visitor,
            attachmentFileIds: attachmentFileIds);
        return await _messageRepository.InsertAsync(message, autoSave: true);
    }
}
