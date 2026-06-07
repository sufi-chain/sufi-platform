using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.Composer;
using SufiChain.Chat.Permissions;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/composer/copilot")]
public class ChatOperatorCopilotController : ChatController, IChatOperatorCopilotAppService
{
    private readonly IChatOperatorCopilotAppService _copilotAppService;

    public ChatOperatorCopilotController(IChatOperatorCopilotAppService copilotAppService)
    {
        _copilotAppService = copilotAppService;
    }

    [HttpPost("assist")]
    [Authorize(ChatPermissions.Inbox.Reply)]
    public virtual Task<ChatOperatorCopilotResultDto> AssistAsync([FromBody] ChatOperatorCopilotInput input)
    {
        return _copilotAppService.AssistAsync(input);
    }
}
