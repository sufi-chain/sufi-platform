using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Permissions;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/assistant")]
public class ChatAssistantController : ChatController, IChatAssistantAvailabilityAppService
{
    private readonly IChatAssistantAvailabilityAppService _assistantAvailabilityAppService;

    public ChatAssistantController(IChatAssistantAvailabilityAppService assistantAvailabilityAppService)
    {
        _assistantAvailabilityAppService = assistantAvailabilityAppService;
    }

    [HttpGet("availability")]
    [Authorize(ChatPermissions.Messages.Send)]
    public virtual Task<ChatAssistantAvailabilityDto> GetAsync()
    {
        return _assistantAvailabilityAppService.GetAsync();
    }
}
