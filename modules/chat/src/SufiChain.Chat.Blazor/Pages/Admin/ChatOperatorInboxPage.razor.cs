using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Blazor.Pages;
using SufiChain.Chat.Composer;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Pages.Admin;
[Authorize(ChatPermissions.Inbox.Operator)]
public partial class ChatOperatorInboxPage : ChatMessengerHostBase
{
    protected ChatSessionStatus? StatusFilter { get; set; } = ChatSessionStatus.Open;

    protected ConversationKind? ConversationKindFilter { get; set; }

    protected AccessMode? AccessModeFilter { get; set; }

    protected bool CanCloseSession { get; set; }

    protected bool IsClosingSession { get; set; }

    protected bool HasActiveFilters =>
        StatusFilter.HasValue || ConversationKindFilter.HasValue || AccessModeFilter.HasValue;

    protected string ActiveFilterSummary => string.Join(
        " · ",
        new[]
        {
            StatusFilter.HasValue ? L[$"SessionStatus:{StatusFilter}"] : null,
            ConversationKindFilter.HasValue ? L[$"ConversationKind:{ConversationKindFilter}"] : null,
            AccessModeFilter.HasValue ? L[$"AccessMode:{AccessModeFilter}"] : null
        }.Where(part => part != null));

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        CanCloseSession = await AuthorizationService.IsGrantedAsync(ChatPermissions.Sessions.Close);
    }

    protected override async Task OnSendMessageAsync(ChatComposerSendRequest request)
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

        await ExecuteWithLoadingAsync(async () =>
        {
            MessengerState.IsSendingMessage = true;
            MessengerState.NotifyStateChanged();

            try
            {
                var sent = await MessageAppService.SendAsync(new SendChatMessageInput
                {
                    SessionId = MessengerState.SelectedSessionId.Value,
                    Body = request.Body.Trim(),
                    SenderKind = ChatMessageSenderKind.Operator,
                    AccessMode = AccessMode.Internal,
                    MetadataJson = request.MetadataJson,
                    AttachmentFileIds = request.AttachmentFileIds
                });

                if (!MessengerState.Messages.Any(item => item.Id == sent.Id))
                {
                    MessengerState.Messages.Add(sent);
                }

                MessengerState.ClearDraft();
            }
            finally
            {
                MessengerState.IsSendingMessage = false;
                MessengerState.NotifyStateChanged();
            }
        }, LoadingKeys.SendMessage);
    }

    protected Task OnOperatorDraftChangedAsync(string draft)
    {
        MessengerState.DraftMessage = draft;
        MessengerState.NotifyStateChanged();
        return Task.CompletedTask;
    }

    protected Task OnGalleryAttachmentsSelectedAsync(IReadOnlyList<Guid> fileIds)
    {
        foreach (var fileId in fileIds)
        {
            if (!MessengerState.DraftAttachmentFileIds.Contains(fileId))
            {
                MessengerState.DraftAttachmentFileIds.Add(fileId);
            }
        }

        MessengerState.NotifyStateChanged();
        return Task.CompletedTask;
    }

    protected override async Task LoadSessionsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            MessengerState.IsLoadingSessions = true;
            MessengerState.NotifyStateChanged();

            try
            {
                var result = await SessionAppService.GetListAsync(new GetChatSessionListInput
                {
                    MaxResultCount = 100,
                    SkipCount = 0,
                    Status = StatusFilter,
                    ConversationKind = ConversationKindFilter,
                    AccessMode = AccessModeFilter
                });

                MessengerState.Sessions = result.Items.ToList();
            }
            finally
            {
                MessengerState.IsLoadingSessions = false;
                MessengerState.NotifyStateChanged();
            }
        }, LoadingKeys.LoadSessions);
    }

    protected override async Task LoadMessagesAsync(Guid sessionId)
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
                IncludeInternal = true,
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

    protected virtual Task RefreshSessionsAsync()
    {
        return LoadSessionsAsync();
    }

    protected virtual Task OnStatusFilterChangedAsync(ChatSessionStatus? value)
    {
        StatusFilter = value;
        return ApplyFiltersAsync();
    }

    protected virtual Task OnConversationKindFilterChangedAsync(ConversationKind? value)
    {
        ConversationKindFilter = value;
        return ApplyFiltersAsync();
    }

    protected virtual Task OnAccessModeFilterChangedAsync(AccessMode? value)
    {
        AccessModeFilter = value;
        return ApplyFiltersAsync();
    }

    protected virtual Task ApplyFiltersAsync()
    {
        MessengerState.SelectedSessionId = null;
        MessengerState.SelectedSession = null;
        MessengerState.Messages.Clear();
        MessengerState.NotifyStateChanged();
        return LoadSessionsAsync();
    }

    protected virtual Task OpenSessionDetailPageAsync()
    {
        if (MessengerState.SelectedSessionId.HasValue)
        {
            NavigationManager.NavigateTo($"/admin/chat/sessions/{MessengerState.SelectedSessionId.Value}");
        }

        return Task.CompletedTask;
    }

    protected virtual async Task CloseSelectedSessionAsync()
    {
        if (!MessengerState.SelectedSessionId.HasValue ||
            MessengerState.SelectedSession?.Status != ChatSessionStatus.Open)
        {
            return;
        }

        var confirmed = await Message.ConfirmAsync(L["CloseSession:Confirm"]);
        if (!confirmed)
        {
            return;
        }

        IsClosingSession = true;
        try
        {
            await SessionAppService.CloseAsync(MessengerState.SelectedSessionId.Value);
            await Message.SuccessAsync(L["CloseSession:Success"]);
            await OnSessionSelectedAsync(MessengerState.SelectedSessionId.Value);
            await LoadSessionsAsync();
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
        finally
        {
            IsClosingSession = false;
        }
    }
}
