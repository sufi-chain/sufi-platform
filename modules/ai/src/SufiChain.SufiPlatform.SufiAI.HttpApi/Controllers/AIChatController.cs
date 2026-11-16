using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.Controllers;

[Area(AIRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai/chat")]
// The app-service [Authorize] runs only after this action starts. Anonymous
// HTTP calls must be rejected here, before a workspace query or a model client.
[Authorize(AIPermissions.AI.Chat)]
public class AIChatController : AIController, ISufiAIChatAppService
{
    private readonly ISufiAIChatAppService _chatAppService;

    public AIChatController(ISufiAIChatAppService chatAppService)
    {
        _chatAppService = chatAppService;
    }

    [HttpPost("messages")]
    public virtual Task<SufiAIChatResponseDto> SendMessageAsync(
        SufiAISendChatMessageInput input,
        CancellationToken cancellationToken = default)
    {
        return _chatAppService.SendMessageAsync(input, cancellationToken);
    }

    [HttpPost("messages/with-tools")]
    [Authorize(AIPermissions.MCPTools.Execute)]
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
