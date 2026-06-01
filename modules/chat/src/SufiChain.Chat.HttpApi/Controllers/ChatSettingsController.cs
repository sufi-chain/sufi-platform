using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Settings;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/settings")]
public class ChatSettingsController : ChatController, IChatSettingsAppService
{
    private readonly IChatSettingsAppService _settingsAppService;

    public ChatSettingsController(IChatSettingsAppService settingsAppService)
    {
        _settingsAppService = settingsAppService;
    }

    [HttpGet]
    [Authorize(ChatPermissions.Settings.Manage)]
    public virtual Task<ChatSettingsDto> GetAsync()
    {
        return _settingsAppService.GetAsync();
    }

    [HttpPut]
    [Authorize(ChatPermissions.Settings.Manage)]
    public virtual Task UpdateAsync([FromBody] UpdateChatSettingsInput input)
    {
        return _settingsAppService.UpdateAsync(input);
    }
}
