using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Pages;

public partial class ChatMessengerPage : ChatMessengerHostBase
{
    protected override async Task LoadSessionsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            MessengerState.IsLoadingSessions = true;
            MessengerState.NotifyStateChanged();

            try
            {
                var result = await SessionAppService.GetMySessionsAsync(new GetMyChatSessionsInput
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
