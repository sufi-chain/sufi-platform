using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Localization;
using SufiChain.Chat.Permissions;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.Chat;

public abstract class ChatAppService : SufiAbpApplicationService
{
    protected ChatAppService()
    {
        LocalizationResource = typeof(ChatResource);
    }

    protected virtual async Task EnsureCanReadMessagesAsync()
    {
        if (await IsGrantedAnyAsync(
            ChatPermissions.Messages.Default,
            ChatPermissions.Inbox.User,
            ChatPermissions.Inbox.Operator,
            ChatPermissions.Inbox.Reply))
        {
            return;
        }

        await CheckPolicyAsync(ChatPermissions.Messages.Default);
    }

    protected virtual async Task EnsureCanSendMessagesAsync()
    {
        if (await IsGrantedAnyAsync(
            ChatPermissions.Messages.Send,
            ChatPermissions.Inbox.User,
            ChatPermissions.Inbox.Reply))
        {
            return;
        }

        await CheckPolicyAsync(ChatPermissions.Messages.Send);
    }

    protected virtual async Task<bool> IsGrantedAnyAsync(params string[] permissionNames)
    {
        foreach (var permissionName in permissionNames)
        {
            if (await AuthorizationService.IsGrantedAsync(permissionName))
            {
                return true;
            }
        }

        return false;
    }
}
