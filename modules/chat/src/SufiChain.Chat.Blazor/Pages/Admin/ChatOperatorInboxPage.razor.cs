using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Blazor.Pages;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Pages.Admin;

[Authorize(ChatPermissions.Inbox.Operator)]
public partial class ChatOperatorInboxPage : ChatMessengerHostBase
{
    protected override async Task OnSendMessageAsync(string message)
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
                    SenderKind = ChatMessageSenderKind.Operator,
                    AccessMode = AccessMode.Internal
                });

                if (!MessengerState.Messages.Any(item => item.Id == sent.Id))
                {
                    MessengerState.Messages.Add(sent);
                }

                MessengerState.DraftMessage = string.Empty;
            }
            finally
            {
                MessengerState.IsSendingMessage = false;
                MessengerState.NotifyStateChanged();
            }
        }, LoadingKeys.SendMessage);
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
                    SkipCount = 0
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
}
