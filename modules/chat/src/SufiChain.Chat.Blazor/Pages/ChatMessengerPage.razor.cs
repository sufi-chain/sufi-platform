using Microsoft.Extensions.Logging;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Pages;

public partial class ChatMessengerPage : ChatMessengerHostBase
{
    protected bool IsAddGroupMembersDialogOpen { get; set; }

    protected bool CanManageGroupMembers =>
        MessengerState.SelectedSession?.ConversationKind == ConversationKind.Group &&
        MessengerState.SelectedSession.CreatorId == CurrentUser.Id;

    protected override async Task LoadSessionsAsync()
    {
        Logger.LogInformation("[CHAT DEBUG] LoadSessionsAsync called");
        
        await ExecuteWithLoadingAsync(async () =>
        {
            MessengerState.IsLoadingSessions = true;
            MessengerState.NotifyStateChanged();

            try
            {
                Logger.LogInformation("[CHAT DEBUG] Calling GetMySessionsAsync...");
                var result = await SessionAppService.GetMySessionsAsync(new GetMyChatSessionsInput
                {
                    MaxResultCount = 100,
                    SkipCount = 0
                });

                Logger.LogInformation("[CHAT DEBUG] Received {Count} sessions from backend", result.Items.Count);
                foreach (var session in result.Items)
                {
                    Logger.LogInformation("[CHAT DEBUG] Session: Id={Id}, Title={Title}, Kind={Kind}, Status={Status}", 
                        session.Id, session.Title, session.ConversationKind, session.Status);
                }
                
                MessengerState.Sessions = result.Items.ToList();
                Logger.LogInformation("[CHAT DEBUG] Sessions updated in state. Total={Total}", MessengerState.Sessions.Count);
            }
            finally
            {
                MessengerState.IsLoadingSessions = false;
                MessengerState.NotifyStateChanged();
            }
        }, LoadingKeys.LoadSessions);
    }

    protected Task OpenAddGroupMembersDialogAsync()
    {
        IsAddGroupMembersDialogOpen = true;
        return Task.CompletedTask;
    }

    protected async Task OnGroupMembersAddedAsync()
    {
        if (!MessengerState.SelectedSessionId.HasValue)
        {
            return;
        }

        await OnSessionSelectedAsync(MessengerState.SelectedSessionId.Value);
        await LoadSessionsAsync();
    }

    protected IReadOnlyCollection<Guid> GetExistingGroupMemberUserIds() =>
        (IReadOnlyCollection<Guid>?)MessengerState.SelectedSession?.Participants
            .Where(participant => participant.UserId.HasValue && participant.LeftAt == null)
            .Select(participant => participant.UserId!.Value)
            .ToList()
        ?? Array.Empty<Guid>();
}
