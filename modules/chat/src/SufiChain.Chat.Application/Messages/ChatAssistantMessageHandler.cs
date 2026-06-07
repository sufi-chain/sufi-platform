using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.ETOs;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Realtime;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Mapping;
using SufiChain.SufiAbp.AIManagement.AI;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;
using AIChatMessageDto = SufiChain.SufiAbp.AIManagement.AI.ChatMessageDto;
using AISendChatMessageInput = SufiChain.SufiAbp.AIManagement.AI.SendChatMessageInput;

namespace SufiChain.Chat.Application.Messages;

/// <summary>
/// Handles incoming user messages in AI Assistant conversations and generates AI responses.
/// </summary>
public class ChatAssistantMessageHandler : 
    IDistributedEventHandler<ChatMessageSentEto>, 
    ITransientDependency
{
    protected ILogger<ChatAssistantMessageHandler> Logger { get; }
    protected IChatSessionRepository SessionRepository { get; }
    protected IChatMessageRepository MessageRepository { get; }
    protected ChatMessageManager MessageManager { get; }
    protected IChatRealtimeNotifier RealtimeNotifier { get; }
    protected ChatApplicationMapper Mapper { get; }
    protected IAIAppService AIAppService { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    public ChatAssistantMessageHandler(
        ILogger<ChatAssistantMessageHandler> logger,
        IChatSessionRepository sessionRepository,
        IChatMessageRepository messageRepository,
        ChatMessageManager messageManager,
        IChatRealtimeNotifier realtimeNotifier,
        ChatApplicationMapper mapper,
        IAIAppService aiAppService,
        IUnitOfWorkManager unitOfWorkManager)
    {
        Logger = logger;
        SessionRepository = sessionRepository;
        MessageRepository = messageRepository;
        MessageManager = messageManager;
        RealtimeNotifier = realtimeNotifier;
        Mapper = mapper;
        AIAppService = aiAppService;
        UnitOfWorkManager = unitOfWorkManager;
    }

    public async Task HandleEventAsync(ChatMessageSentEto eventData)
    {
        Logger.LogInformation("[CHAT DEBUG HANDLER] ChatMessageSentEto received. MessageId={MessageId}, SessionId={SessionId}, SenderKind={SenderKind}", 
            eventData.Id, eventData.SessionId, eventData.SenderKind);

        // Only process user/visitor messages, not assistant messages
        if (eventData.SenderKind == ChatMessageSenderKind.Assistant || 
            eventData.SenderKind == ChatMessageSenderKind.System)
        {
            Logger.LogInformation("[CHAT DEBUG HANDLER] Skipping - message is from Assistant or System");
            return;
        }

        // Don't create a new UoW - use the ambient one so we can see the uncommitted message
        try
        {
            var session = await SessionRepository.GetAsync(eventData.SessionId);
            
            Logger.LogInformation("[CHAT DEBUG HANDLER] Session loaded. ConversationKind={Kind}, Status={Status}", 
                session.ConversationKind, session.Status);

            // Only process Assistant conversations
            if (session.ConversationKind != ConversationKind.Assistant)
            {
                Logger.LogInformation("[CHAT DEBUG HANDLER] Skipping - not an Assistant conversation");
                return;
            }

            if (session.Status == ChatSessionStatus.Closed)
            {
                Logger.LogInformation("[CHAT DEBUG HANDLER] Skipping - session is closed");
                return;
            }

            if (ChatAssistantMetadata.IsExternallyOrchestrated(session.MetadataJson))
            {
                Logger.LogInformation("[CHAT DEBUG HANDLER] Skipping - assistant session is externally orchestrated");
                return;
            }

            // Get the user's message - it should be in the current UoW context
            var userMessage = await MessageRepository.GetAsync(eventData.Id);
            
            Logger.LogInformation("[CHAT DEBUG HANDLER] User message loaded. Body length={Length}", 
                userMessage.Body?.Length ?? 0);

            // Extract workspace name from session metadata
            var workspaceName = ChatAssistantMetadata.TryGetWorkspaceName(session.MetadataJson);
            if (string.IsNullOrWhiteSpace(workspaceName))
            {
                Logger.LogWarning("[CHAT DEBUG HANDLER] No workspace configured for AI session. SessionId={SessionId}", 
                    session.Id);
                return;
            }

            Logger.LogInformation("[CHAT DEBUG HANDLER] Workspace={Workspace}. Loading conversation history...", 
                workspaceName);

            // Get conversation history (last 20 messages)
            var historyMessages = await MessageRepository.GetListBySessionAsync(
                session.Id,
                includeInternal: false,
                skipCount: 0,
                maxResultCount: 20);

            var conversationHistory = historyMessages
                .Where(m => m.Id != eventData.Id) // Exclude current message
                .OrderBy(m => m.CreationTime)
                .Select(m => new AIChatMessageDto
                {
                    Role = m.SenderKind == ChatMessageSenderKind.Assistant ? "assistant" : "user",
                    Content = m.Body ?? string.Empty
                })
                .ToList();

            Logger.LogInformation("[CHAT DEBUG HANDLER] Conversation history: {Count} messages. Calling AI service...", 
                conversationHistory.Count);

            // Call AI service to get response
            var aiResponse = await AIAppService.SendChatMessageAsync(new AISendChatMessageInput
            {
                WorkspaceName = workspaceName,
                Message = userMessage.Body ?? string.Empty,
                ConversationHistory = conversationHistory,
                Stream = false // For event handler, use non-streaming
            });

            Logger.LogInformation("[CHAT DEBUG HANDLER] AI response received. Length={Length}, Tokens={Tokens}", 
                aiResponse.Message?.Length ?? 0, aiResponse.TokensUsed);

            // Send AI response as a new message
            var assistantMessage = await MessageManager.SendAsync(
                session,
                aiResponse.Message ?? string.Empty,
                ChatMessageSenderKind.Assistant,
                senderUserId: null,
                anonymousVisitorId: null,
                isInternal: false,
                isAuthorizedOperator: true,
                metadataJson: null,
                attachmentFileIds: null);

            Logger.LogInformation("[CHAT DEBUG HANDLER] Assistant message created. MessageId={MessageId}", 
                assistantMessage.Id);

            // Broadcast the assistant's response via SignalR
            var messageDto = Mapper.ToDto(assistantMessage);
            await RealtimeNotifier.NotifyMessageSentAsync(messageDto);

            Logger.LogInformation("[CHAT DEBUG HANDLER] Assistant response broadcast via SignalR. MessageId={MessageId}", 
                assistantMessage.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[CHAT DEBUG HANDLER] Error processing AI assistant message. SessionId={SessionId}, MessageId={MessageId}", 
                eventData.SessionId, eventData.Id);
            // Don't throw - we don't want to break the event bus
        }
    }
}
