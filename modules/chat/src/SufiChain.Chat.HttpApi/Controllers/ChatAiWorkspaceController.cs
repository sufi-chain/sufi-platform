using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Permissions;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/ai-workspace")]
public class ChatAiWorkspaceController : ChatController, IChatAiWorkspaceSelectionAppService
{
    private readonly IChatAiWorkspaceSelectionAppService _workspaceSelectionAppService;

    public ChatAiWorkspaceController(IChatAiWorkspaceSelectionAppService workspaceSelectionAppService)
    {
        _workspaceSelectionAppService = workspaceSelectionAppService;
    }

    [HttpGet]
    [Authorize(ChatPermissions.Messages.Send)]
    public virtual Task<ChatAiWorkspaceSelectionDto> GetAsync()
    {
        return _workspaceSelectionAppService.GetAsync();
    }

    [HttpGet("options")]
    [Authorize(ChatPermissions.Messages.Send)]
    public virtual Task<List<ChatAiWorkspaceOptionDto>> GetOptionsAsync()
    {
        return _workspaceSelectionAppService.GetOptionsAsync();
    }

    [HttpPut("default")]
    [Authorize(ChatPermissions.Settings.Manage)]
    public virtual Task UpdateDefaultAsync([FromBody] UpdateChatAiWorkspaceSelectionInput input)
    {
        return _workspaceSelectionAppService.UpdateDefaultAsync(input);
    }
}
