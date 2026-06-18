using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.AIManagement.AI;
using Volo.Abp;

namespace SufiChain.SufiAbp.AIManagement.Controllers;

[Area(AIManagementRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIManagementRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai-management/chat")]
public class AIChatController : AIManagementController, ISufiAbpAIChatAppService
{
    private readonly ISufiAbpAIChatAppService _chatAppService;

    public AIChatController(ISufiAbpAIChatAppService chatAppService)
    {
        _chatAppService = chatAppService;
    }

    [HttpPost("messages")]
    public virtual Task<SufiAbpAIChatResponseDto> SendMessageAsync(SufiAbpAISendChatMessageInput input)
    {
        return _chatAppService.SendMessageAsync(input);
    }

    [HttpPost("messages/with-tools")]
    public virtual Task<SufiAbpAIChatResponseDto> SendMessageWithToolsAsync(SufiAbpAISendChatMessageInput input)
    {
        return _chatAppService.SendMessageWithToolsAsync(input);
    }

    [HttpPost("messages/stream")]
    public virtual IAsyncEnumerable<SufiAbpAIChatResponseDto> StreamMessageAsync(SufiAbpAISendChatMessageInput input)
    {
        return _chatAppService.StreamMessageAsync(input);
    }
}
