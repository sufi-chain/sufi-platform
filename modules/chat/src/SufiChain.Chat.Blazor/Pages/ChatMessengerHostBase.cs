using Microsoft.AspNetCore.Components;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Blazor.Public;
using SufiChain.Chat.Blazor.Public.Services;
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
    protected ChatMessengerState MessengerState { get; set; } = default!;

    [Inject]
    protected IChatHubClientService HubClientService { get; set; } = default!;

    [Inject]
    protected IFeatureChecker FeatureChecker { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected ChatAssistantAvailabilityDto? AssistantAvailability { get; set; }

    protected bool ShowStartAiChat =>
        AssistantAvailability?.IsAvailable == true;

    protected string? AssistantUnavailableMessageKey =>
        AssistantAvailability?.IsAvailable == false
            ? AssistantAvailability.MessageKey ?? "Chat:AiUnavailable"
            : null;

    protected bool IsNewDirectMessageDialogOpen { get; set; }

    protected bool IsNewGroupDialogOpen { get; set; }

    protected bool CanCreateDirectMessages { get; set; }

    protected bool CanCreateGroups { get; set; }

    protected Guid? ActiveHubSessionId { get; set; }

    protected static class LoadingKeys
    {
        public const string LoadSessions = "load-sessions";
        public const string LoadMessages = "load-messages";
        public const string SendMessage = "send-message";
        public const string StartAiChat = "start-ai-chat";
    }

    protected override async Task OnInitializedAsync()
    {
        HubClientService.MessageReceived += OnMessageReceivedAsync;
        HubClientService.SessionUpdated += OnSessionUpdatedAsync;
        HubClientService.UsageLimitExceeded += OnUsageLimitExceededAsync;

        CanCreateDirectMessages = await FeatureChecker.IsEnabledAsync(ChatFeatures.Enable);
        CanCreateGroups = await FeatureChecker.IsEnabledAsync(ChatFeatures.Enable);

        await LoadAssistantAvailabilityAsync();
        await LoadSessionsAsync();
    }

    protected abstract Task LoadSessionsAsync();

    protected virtual async Task LoadAssistantAvailabilityAsync()
    {
        AssistantAvailability = await AssistantAvailabilityAppService.GetAsync();
    }

    protected virtual async Task OnSessionSelectedAsync(Guid sessionId)
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
            await HubClientService.EnsureConnectedAsync();
            await HubClientService.JoinSessionAsync(sessionId);
            ActiveHubSessionId = sessionId;
            MessengerState.NotifyStateChanged();
        }, LoadingKeys.LoadMessages);
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

    protected virtual async Task OnSendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || !MessengerState.SelectedSessionId.HasValue)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            MessengerState.IsSendingMessage = true;
            MessengerState.NotifyStateChanged();

            try
            {
                var sent = await MessageAppService.SendAsync(new SendChatMessageInput
                {
                    SessionId = MessengerState.SelectedSessionId.Value,
                    Body = message.Trim(),
                    SenderKind = ChatMessageSenderKind.Visitor,
                    AccessMode = AccessMode.PublicAuthenticated
                });

                if (!MessengerState.Messages.Any(item => item.Id == sent.Id))
                {
                    MessengerState.Messages.Add(sent);
                }

                MessengerState.DraftMessage = string.Empty;
                MessengerState.ClearSignupRequired();
            }
            finally
            {
                MessengerState.IsSendingMessage = false;
                MessengerState.NotifyStateChanged();
            }
        }, LoadingKeys.SendMessage);
    }

    protected virtual async Task StartAiChatAsync()
    {
        if (!ShowStartAiChat)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            var existing = MessengerState.Sessions.FirstOrDefault(session =>
                session.ConversationKind == ConversationKind.Assistant &&
                session.Status != ChatSessionStatus.Closed);

            ChatSessionDto session;
            if (existing != null)
            {
                session = await SessionAppService.GetAsync(existing.Id);
            }
            else
            {
                session = await SessionAppService.CreateAsync(new CreateChatSessionInput
                {
                    Title = L["Menu:StartAiChat"],
                    AccessMode = AccessMode.PublicAuthenticated,
                    ConversationKind = ConversationKind.Assistant,
                    ChannelOrigin = ChannelOrigin.Web
                });

                await LoadSessionsAsync();
            }

            await OnSessionSelectedAsync(session.Id);
        }, LoadingKeys.StartAiChat);
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

        if (MessengerState.Messages.All(item => item.Id != message.Id))
        {
            MessengerState.Messages.Add(message);
            MessengerState.NotifyStateChanged();
        }

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
    }
}
