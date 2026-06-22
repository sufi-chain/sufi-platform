using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI;
using Volo.Abp;

namespace SufiChain.SufiAbp.AI.Controllers;

[Area(AIRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai/chat")]
public class AIChatController : AIController, ISufiAIChatAppService
{
    private readonly ISufiAIChatAppService _chatAppService;

    public AIChatController(ISufiAIChatAppService chatAppService)
    {
        _chatAppService = chatAppService;
    }

    [HttpPost("messages")]
    public virtual Task<SufiAIChatResponseDto> SendMessageAsync(SufiAISendChatMessageInput input)
    {
        return _chatAppService.SendMessageAsync(input);
    }

    [HttpPost("messages/with-tools")]
    public virtual Task<SufiAIChatResponseDto> SendMessageWithToolsAsync(SufiAISendChatMessageInput input)
    {
        return _chatAppService.SendMessageWithToolsAsync(input);
    }

    [HttpPost("messages/stream")]
    public virtual IAsyncEnumerable<SufiAIChatResponseDto> StreamMessageAsync(SufiAISendChatMessageInput input)
    {
        return _chatAppService.StreamMessageAsync(input);
    }
}
