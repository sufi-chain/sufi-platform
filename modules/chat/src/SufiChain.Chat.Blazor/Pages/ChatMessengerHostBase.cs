using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Blazor.Public;
using SufiChain.Chat.Blazor.Public.Services;
using SufiChain.Chat.Composer;
using SufiChain.Chat.Features;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using SufiChain.SufiAbp.Features;

namespace SufiChain.Chat.Blazor.Pages;

/// <summary>
/// Shared messenger page logic for user and operator inbox pages.
/// </summary>
public abstract class ChatMessengerHostBase : ChatComponentBase, IAsyncDisposable
{
    [Inject]
    protected IChatSessionAppService SessionAppService { get; set; } = default!;

    [Inject]
    protected IChatMessageAppService MessageAppService { get; set; } = default!;

    [Inject]
    protected IChatAssistantAvailabilityAppService AssistantAvailabilityAppService { get; set; } = default!;

    [Inject]
    protected IChatAssistantWorkspaceResolver AssistantWorkspaceResolver { get; set; } = default!;

    [Inject]
    protected ChatMessengerState MessengerState { get; set; } = default!;

    [Inject]
    protected IChatHubClientService HubClientService { get; set; } = default!;

    [Inject]
    protected IFeatureChecker FeatureChecker { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    protected IChatComposerCapabilitiesAppService ComposerCapabilitiesAppService { get; set; } = default!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "assistantKey")]
    public string? AssistantKey { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "assistant")]
    public string? Assistant { get; set; }

    protected ChatAssistantAvailabilityDto? AssistantAvailability { get; set; }

    protected bool ShowStartAiChat =>
        AssistantAvailability?.IsAvailable == true;

    protected IReadOnlyList<ChatAssistantPickerOptionDto> AssistantPickerOptions =>
        (IReadOnlyList<ChatAssistantPickerOptionDto>?)AssistantAvailability?.Assistants
        ?? Array.Empty<ChatAssistantPickerOptionDto>();

    protected string? AssistantUnavailableMessageKey =>
        AssistantAvailability?.IsAvailable == false
            ? AssistantAvailability.MessageKey ?? "Chat:AiUnavailable"
            : null;

    protected bool IsNewDirectMessageDialogOpen { get; set; }

    protected bool IsNewGroupDialogOpen { get; set; }

    protected bool CanCreateDirectMessages { get; set; }

    protected bool CanCreateGroups { get; set; }

    protected Guid? ActiveHubSessionId { get; set; }

    private readonly SemaphoreSlim _sessionSelectionLock = new(1, 1);

    protected static class LoadingKeys
    {
        public const string LoadSessions = "load-sessions";
        public const string LoadMessages = "load-messages";
        public const string SendMessage = "send-message";
        public const string StartAiChat = "start-ai-chat";
    }

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("[CHAT DEBUG] ChatMessengerHostBase.OnInitializedAsync starting...");
        
        HubClientService.MessageReceived += OnMessageReceivedAsync;
        HubClientService.SessionUpdated += OnSessionUpdatedAsync;
        HubClientService.UsageLimitExceeded += OnUsageLimitExceededAsync;
        Logger.LogInformation("[CHAT DEBUG] SignalR event handlers registered");

        CanCreateDirectMessages = await FeatureChecker.IsEnabledAsync(ChatFeatures.Enable);
        CanCreateGroups = await FeatureChecker.IsEnabledAsync(ChatFeatures.Enable);

        Logger.LogInformation("[CHAT DEBUG] Loading assistant availability...");
        await LoadAssistantAvailabilityAsync();
        Logger.LogInformation("[CHAT DEBUG] Assistant availability loaded. IsAvailable={IsAvailable}", 
            AssistantAvailability?.IsAvailable);
        
        Logger.LogInformation("[CHAT DEBUG] Loading sessions...");
        await LoadSessionsAsync();

        var requestedAssistantKey = ResolveStartupAssistantKey();
        if (!string.IsNullOrWhiteSpace(requestedAssistantKey))
        {
            Logger.LogInformation("[CHAT DEBUG] Starting assistant from page parameter. AssistantKey={AssistantKey}", requestedAssistantKey);
            await StartAssistantChatAsync(requestedAssistantKey);
        }

        Logger.LogInformation("[CHAT DEBUG] OnInitializedAsync complete");
    }

    protected abstract Task LoadSessionsAsync();

    protected virtual async Task LoadAssistantAvailabilityAsync()
    {
        AssistantAvailability = await AssistantAvailabilityAppService.GetAsync();
    }

    protected virtual async Task OnSessionSelectedAsync(Guid sessionId)
    {
        await _sessionSelectionLock.WaitAsync(ComponentCancellationToken);
        try
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                if (ActiveHubSessionId.HasValue && ActiveHubSessionId != sessionId)
                {
                    await HubClientService.LeaveSessionAsync(ActiveHubSessionId.Value);
                }

                MessengerState.SelectedSessionId = sessionId;
                MessengerState.SelectedSession = await SessionAppService.GetAsync(sessionId);
                MessengerState.SelectSession(sessionId);

                await LoadMessagesAsync(sessionId);
                await LoadComposerCapabilitiesAsync(sessionId);
                await HubClientService.EnsureConnectedAsync();
                await HubClientService.JoinSessionAsync(sessionId);
                ActiveHubSessionId = sessionId;
                MessengerState.NotifyStateChanged();
            }, LoadingKeys.LoadMessages);
        }
        finally
        {
            _sessionSelectionLock.Release();
        }
    }

    protected virtual async Task LoadMessagesAsync(Guid sessionId)
    {
        MessengerState.IsLoadingMessages = true;
        MessengerState.NotifyStateChanged();

        try
        {
            var result = await MessageAppService.GetListAsync(new GetChatMessageListInput
            {
                SessionId = sessionId,
                MaxResultCount = 200,
                SkipCount = 0,
                Sorting = "CreationTime"
            });

            MessengerState.Messages = result.Items.ToList();
        }
        finally
        {
            MessengerState.IsLoadingMessages = false;
            MessengerState.NotifyStateChanged();
        }
    }

    protected virtual async Task LoadComposerCapabilitiesAsync(Guid sessionId)
    {
        MessengerState.ComposerCapabilities = await ComposerCapabilitiesAppService.GetAsync(sessionId);
        MessengerState.NotifyStateChanged();
    }

    protected virtual async Task OnSendMessageAsync(ChatComposerSendRequest request)
    {
        if (!MessengerState.SelectedSessionId.HasValue)
        {
            return;
        }

        var hasContent = !string.IsNullOrWhiteSpace(request.Body) ||
                         request.AttachmentFileIds.Count > 0 ||
                         !string.IsNullOrWhiteSpace(request.MetadataJson);

        if (!hasContent)
        {
            return;
        }

        var isAiConversation = MessengerState.SelectedSession?.ConversationKind == ConversationKind.Assistant;
        Logger.LogInformation("[CHAT DEBUG] OnSendMessageAsync called. SessionId={SessionId}, IsAiConversation={IsAi}",
            MessengerState.SelectedSessionId, isAiConversation);

        try
        {
            MessengerState.IsSendingMessage = true;

            if (isAiConversation)
            {
                Logger.LogInformation("[CHAT DEBUG] Setting IsWaitingForAiResponse=true before sending AI message");
                MessengerState.IsWaitingForAiResponse = true;
                StartAiResponseTimeout();
            }

            MessengerState.NotifyStateChanged();
            await InvokeAsync(StateHasChanged);

            Logger.LogInformation("[CHAT DEBUG] Sending message to backend...");
            var sent = await MessageAppService.SendAsync(BuildSendInput(request));
            Logger.LogInformation("[CHAT DEBUG] Message sent successfully. MessageId={MessageId}", sent.Id);

            if (!MessengerState.Messages.Any(item => item.Id == sent.Id))
            {
                MessengerState.Messages.Add(sent);
            }

            MessengerState.ClearDraft();
            MessengerState.ClearSignupRequired();
            MessengerState.NotifyStateChanged();
        }
        catch (Exception ex)
        {
            if (isAiConversation)
            {
                MessengerState.IsWaitingForAiResponse = false;
                MessengerState.NotifyStateChanged();
            }

            Logger.LogException(ex);
            await Notify.ErrorAsync(L["Chat:MessageSendFailed"], L["Error"]);
        }
        finally
        {
            MessengerState.IsSendingMessage = false;
            MessengerState.NotifyStateChanged();
            await InvokeAsync(StateHasChanged);
        }
    }

    protected virtual void StartAiResponseTimeout()
    {
        _ = Task.Delay(TimeSpan.FromSeconds(30), ComponentCancellationToken)
            .ContinueWith(async _ =>
            {
                if (MessengerState.IsWaitingForAiResponse)
                {
                    Logger.LogWarning("[CHAT DEBUG] AI response timeout after 30 seconds. No response received.");
                    MessengerState.IsWaitingForAiResponse = false;
                    MessengerState.NotifyStateChanged();
                    await InvokeAsync(StateHasChanged);
                }
            }, TaskScheduler.Default);
    }

    protected virtual SendChatMessageInput BuildSendInput(ChatComposerSendRequest request)
    {
        return new SendChatMessageInput
        {
            SessionId = MessengerState.SelectedSessionId!.Value,
            Body = request.Body.Trim(),
            SenderKind = ChatMessageSenderKind.Visitor,
            SenderUserId = CurrentUser.Id,
            AccessMode = AccessMode.PublicAuthenticated,
            MetadataJson = request.MetadataJson,
            AttachmentFileIds = request.AttachmentFileIds
        };
    }

    protected virtual Task StartAiChatAsync()
    {
        return StartAssistantChatAsync(null);
    }

    protected virtual async Task StartAssistantChatAsync(string? assistantKey)
    {
        if (!ShowStartAiChat)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            var existing = await FindExistingAssistantSessionAsync(assistantKey);

            ChatSessionDto session;
            if (existing != null)
            {
                Logger.LogInformation("[CHAT DEBUG] Reusing assistant session. SessionId={SessionId}, AssistantKey={AssistantKey}",
                    existing.Id, assistantKey);
                session = await SessionAppService.GetAsync(existing.Id);
            }
            else
            {
                var workspaceName = await AssistantWorkspaceResolver.ResolveWorkspaceNameAsync(
                    new ChatAssistantWorkspaceResolveContext
                    {
                        AccessMode = AccessMode.PublicAuthenticated,
                        ConversationKind = ConversationKind.Assistant,
                        AssistantKey = assistantKey
                    });

                var sessionTitle = ResolveAssistantSessionTitle(assistantKey);
                var createInput = new CreateChatSessionInput
                {
                    Title = sessionTitle,
                    AccessMode = AccessMode.PublicAuthenticated,
                    ConversationKind = ConversationKind.Assistant,
                    ChannelOrigin = ChannelOrigin.Web,
                    MetadataJson = BuildAssistantSessionMetadata(workspaceName, assistantKey)
                };

                if (CurrentUser.Id.HasValue)
                {
                    createInput.Participants.Add(new AddChatParticipantInput
                    {
                        UserId = CurrentUser.Id,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    });
                }

                session = await SessionAppService.CreateAsync(createInput);
                Logger.LogInformation("[CHAT DEBUG] AI session created successfully. SessionId={SessionId}, Title={Title}", 
                    session.Id, session.Title);

                // Add the session to the list immediately so it appears in the UI
                var listDto = new ChatSessionListDto
                {
                    Id = session.Id,
                    TenantId = session.TenantId,
                    Title = session.Title,
                    AccessMode = session.AccessMode,
                    ConversationKind = session.ConversationKind,
                    ChannelOrigin = session.ChannelOrigin,
                    Status = session.Status,
                    MetadataJson = session.MetadataJson,
                    LastMessageTime = session.LastMessageTime,
                    ParticipantCount = session.Participants.Count,
                    CreationTime = session.CreationTime
                };
                MessengerState.Sessions.Insert(0, listDto);
                MessengerState.NotifyStateChanged();


                //                 await LoadSessionsAsync();
            }

            Logger.LogInformation("[CHAT DEBUG] Calling OnSessionSelectedAsync. SessionId={SessionId}", session.Id);
            await OnSessionSelectedAsync(session.Id);
        }, LoadingKeys.StartAiChat);
    }

    protected virtual string ResolveAssistantSessionTitle(string? assistantKey)
    {
        if (!string.IsNullOrWhiteSpace(assistantKey))
        {
            var assistant = AssistantPickerOptions.FirstOrDefault(item =>
                item.Key.Equals(assistantKey, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(assistant?.DisplayName))
            {
                return $"{assistant.DisplayName}";
            }
        }

        // Default: "AI Assistant - New Conversation"
        return $"{L["Chat:AIAssistant"]}";
    }

    protected virtual async Task<ChatSessionListDto?> FindExistingAssistantSessionAsync(string? assistantKey)
    {
        var normalizedAssistantKey = NormalizeAssistantKey(assistantKey);
        var candidateSessions = MessengerState.Sessions
            .Where(session =>
                session.ConversationKind == ConversationKind.Assistant &&
                session.Status != ChatSessionStatus.Closed)
            .ToList();

        if (normalizedAssistantKey == null)
        {
            foreach (var candidateSession in candidateSessions)
            {
                var metadataAssistantKey = ChatAssistantMetadata.TryGetAssistantKey(candidateSession.MetadataJson);
                if (string.IsNullOrWhiteSpace(metadataAssistantKey) && !string.IsNullOrWhiteSpace(candidateSession.MetadataJson))
                {
                    return candidateSession;
                }

                if (string.IsNullOrWhiteSpace(candidateSession.MetadataJson))
                {
                    var fullSession = await SessionAppService.GetAsync(candidateSession.Id);
                    metadataAssistantKey = ChatAssistantMetadata.TryGetAssistantKey(fullSession.MetadataJson);
                    candidateSession.MetadataJson = fullSession.MetadataJson;
                    if (string.IsNullOrWhiteSpace(metadataAssistantKey))
                    {
                        return candidateSession;
                    }
                }
            }

            return null;
        }

        foreach (var candidateSession in candidateSessions)
        {
            var metadataAssistantKey = ChatAssistantMetadata.TryGetAssistantKey(candidateSession.MetadataJson);
            if (normalizedAssistantKey.Equals(metadataAssistantKey, StringComparison.OrdinalIgnoreCase))
            {
                return candidateSession;
            }

            if (string.IsNullOrWhiteSpace(candidateSession.MetadataJson))
            {
                var fullSession = await SessionAppService.GetAsync(candidateSession.Id);
                metadataAssistantKey = ChatAssistantMetadata.TryGetAssistantKey(fullSession.MetadataJson);
                if (normalizedAssistantKey.Equals(metadataAssistantKey, StringComparison.OrdinalIgnoreCase))
                {
                    candidateSession.MetadataJson = fullSession.MetadataJson;
                    return candidateSession;
                }
            }
        }

        return null;
    }

    protected virtual string? ResolveStartupAssistantKey()
    {
        var assistantKey = NormalizeAssistantKey(AssistantKey) ?? NormalizeAssistantKey(Assistant);
        if (assistantKey != null)
        {
            return assistantKey;
        }

        return null;
    }

    protected static string? NormalizeAssistantKey(string? assistantKey)
    {
        return string.IsNullOrWhiteSpace(assistantKey)
            ? null
            : assistantKey.Trim().ToLowerInvariant();
    }

    protected static string? BuildAssistantSessionMetadata(string? workspaceName, string? assistantKey)
    {
        if (string.IsNullOrWhiteSpace(workspaceName) && string.IsNullOrWhiteSpace(assistantKey))
        {
            return null;
        }

        return ChatAssistantMetadata.BuildJson(workspaceName ?? string.Empty, assistantKey);
    }

    protected virtual Task OpenNewDirectMessageDialogAsync()
    {
        IsNewDirectMessageDialogOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task CloseNewDirectMessageDialogAsync()
    {
        IsNewDirectMessageDialogOpen = false;
        return Task.CompletedTask;
    }

    protected virtual Task OpenNewGroupDialogAsync()
    {
        IsNewGroupDialogOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task CloseNewGroupDialogAsync()
    {
        IsNewGroupDialogOpen = false;
        return Task.CompletedTask;
    }

    protected virtual async Task OnDirectMessageCreatedAsync(Guid sessionId)
    {
        IsNewDirectMessageDialogOpen = false;
        await LoadSessionsAsync();
        await OnSessionSelectedAsync(sessionId);
    }

    protected virtual async Task OnGroupCreatedAsync(Guid sessionId)
    {
        IsNewGroupDialogOpen = false;
        await LoadSessionsAsync();
        await OnSessionSelectedAsync(sessionId);
    }

    protected virtual Task OnMessageReceivedAsync(ChatMessageDto message)
    {
        if (MessengerState.SelectedSessionId != message.SessionId)
        {
            return Task.CompletedTask;
        }

        Logger.LogInformation("[CHAT DEBUG] Message received. SenderKind={SenderKind}, IsWaitingBefore={IsWaiting}",
            message.SenderKind, MessengerState.IsWaitingForAiResponse);

        if (message.SenderKind == ChatMessageSenderKind.Assistant && MessengerState.IsWaitingForAiResponse)
        {
            MessengerState.IsWaitingForAiResponse = false;
            Logger.LogInformation("[CHAT DEBUG] Cleared IsWaitingForAiResponse on AI response");
        }

        if (MessengerState.Messages.All(item => item.Id != message.Id))
        {
            MessengerState.Messages.Add(message);
        }

        MessengerState.NotifyStateChanged();
        return InvokeAsync(StateHasChanged);
    }

    protected virtual async Task OnSessionUpdatedAsync(ChatSessionDto session)
    {
        var index = MessengerState.Sessions.FindIndex(item => item.Id == session.Id);
        if (index >= 0)
        {
            MessengerState.Sessions[index] = new ChatSessionListDto
            {
                Id = session.Id,
                TenantId = session.TenantId,
                Title = session.Title,
                AccessMode = session.AccessMode,
                ConversationKind = session.ConversationKind,
                ChannelOrigin = session.ChannelOrigin,
                Status = session.Status,
                MetadataJson = session.MetadataJson,
                LastMessageTime = session.LastMessageTime,
                ParticipantCount = session.Participants.Count,
                CreationTime = session.CreationTime
            };
        }

        if (MessengerState.SelectedSessionId == session.Id)
        {
            MessengerState.SelectedSession = session;
        }

        MessengerState.NotifyStateChanged();
        await InvokeAsync(StateHasChanged);
    }

    protected virtual Task OnUsageLimitExceededAsync(ChatUsageCheckResultDto result)
    {
        MessengerState.ApplyUsageLimit(result);
        return InvokeAsync(StateHasChanged);
    }

    protected string? GetSignInUrl()
    {
        return NavigationManager.ToAbsoluteUri("/account/login").ToString();
    }

    public async ValueTask DisposeAsync()
    {
        HubClientService.MessageReceived -= OnMessageReceivedAsync;
        HubClientService.SessionUpdated -= OnSessionUpdatedAsync;
        HubClientService.UsageLimitExceeded -= OnUsageLimitExceededAsync;

        if (ActiveHubSessionId.HasValue)
        {
            await HubClientService.LeaveSessionAsync(ActiveHubSessionId.Value);
        }

        _sessionSelectionLock.Dispose();
    }
}
