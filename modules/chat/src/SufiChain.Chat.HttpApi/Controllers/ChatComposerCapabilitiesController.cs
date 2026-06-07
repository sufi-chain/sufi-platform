using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.Composer;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/composer/capabilities")]
public class ChatComposerCapabilitiesController : ChatController, IChatComposerCapabilitiesAppService
{
    private readonly IChatComposerCapabilitiesAppService _capabilitiesAppService;

    public ChatComposerCapabilitiesController(IChatComposerCapabilitiesAppService capabilitiesAppService)
    {
        _capabilitiesAppService = capabilitiesAppService;
    }

    [HttpGet]
    [Authorize]
    public virtual Task<ChatComposerCapabilitiesDto> GetAsync([FromQuery] Guid? sessionId = null)
    {
        return _capabilitiesAppService.GetAsync(sessionId);
    }
}
