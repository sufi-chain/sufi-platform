using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Chat.Application.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.ETOs;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Chat.Supports;
using Shouldly;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Messages;

/// <summary>
/// 9.4 (sufi-chain#340): the assistant turn has no 60s cutoff. A slow model finishes,
/// a caller cancel does not store a reply, and a session closed during the turn drops it.
/// </summary>
public class ChatAssistantTurnCutoff_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatSessionAppService _sessionAppService;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatMessageRepository _messageRepository;
    private readonly ChatAssistantMessageHandler _handler;
    private readonly ConfigurableAiService _ai;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public ChatAssistantTurnCutoff_Tests()
    {
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _sessionRepository = GetRequiredService<IChatSessionRepository>();
        _messageRepository = GetRequiredService<IChatMessageRepository>();
        _handler = GetRequiredService<ChatAssistantMessageHandler>();
        _ai = GetRequiredService<ConfigurableAiService>();
        _cancellationTokenProvider = GetRequiredService<ICancellationTokenProvider>();
        _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
    }

    [Fact]
    public async Task A_slow_model_reply_is_stored()
    {
        var previousContent = _ai.ResponseContent;
        var previousDelay = _ai.CompleteDelay;
        _ai.ResponseContent = "slow reply";
        _ai.CompleteDelay = TimeSpan.FromSeconds(2);
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(GetRequiredService<ISettingManager>());
            var session = await CreateAssistantSessionAsync();
            var userMessage = await InsertVisitorMessageAsync(session.Id, "hello");

            await _handler.ProcessAsync(EventFor(session.Id, userMessage.Id));

            var messages = await _messageRepository.GetListBySessionAsync(session.Id, includeInternal: false);
            messages.ShouldContain(message =>
                message.SenderKind == ChatMessageSenderKind.Assistant && message.Body == "slow reply");
        }
        finally
        {
            _ai.ResponseContent = previousContent;
            _ai.CompleteDelay = previousDelay;
            _ai.BeforeComplete = null;
        }
    }

    [Fact]
    public async Task Caller_cancellation_does_not_store_a_reply()
    {
        var previous = _ai.BeforeComplete;
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(GetRequiredService<ISettingManager>());
            var session = await CreateAssistantSessionAsync();
            var userMessage = await InsertVisitorMessageAsync(session.Id, "hello");
            using var caller = new CancellationTokenSource();
            caller.Cancel();
            using (_cancellationTokenProvider.Use(caller.Token))
            {
                await _handler.ProcessAsync(EventFor(session.Id, userMessage.Id));
            }

            var messages = await _messageRepository.GetListBySessionAsync(session.Id, includeInternal: false);
            messages.ShouldNotContain(message => message.SenderKind == ChatMessageSenderKind.Assistant);
        }
        finally
        {
            _ai.BeforeComplete = previous;
        }
    }

    [Fact]
    public async Task A_reply_is_dropped_when_the_session_closes_during_the_turn()
    {
        var previous = _ai.BeforeComplete;
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(GetRequiredService<ISettingManager>());
            var session = await CreateAssistantSessionAsync();
            var userMessage = await InsertVisitorMessageAsync(session.Id, "hello");
            _ai.ResponseContent = "late reply";
            _ai.BeforeComplete = async _ =>
            {
                using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
                var live = await _sessionRepository.GetAsync(session.Id);
                live.Close();
                await _sessionRepository.UpdateAsync(live, autoSave: true);
                await uow.CompleteAsync();
            };

            await _handler.ProcessAsync(EventFor(session.Id, userMessage.Id));

            var messages = await _messageRepository.GetListBySessionAsync(session.Id, includeInternal: false);
            messages.ShouldNotContain(message => message.SenderKind == ChatMessageSenderKind.Assistant);
        }
        finally
        {
            _ai.BeforeComplete = previous;
        }
    }

    private static ChatMessageSentEto EventFor(Guid sessionId, Guid messageId) => new()
    {
        Id = Guid.Empty,
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
                Title = "Cutoff",
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
            tenantId: null,
            sessionId,
            body,
            ChatMessageSenderKind.Visitor);
        return await _messageRepository.InsertAsync(message, autoSave: true);
    }
}
