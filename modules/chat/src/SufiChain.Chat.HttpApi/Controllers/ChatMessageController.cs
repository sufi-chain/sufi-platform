using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Permissions;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/messages")]
public class ChatMessageController : ChatController, IChatMessageAppService
{
    private readonly IChatMessageAppService _messageAppService;

    public ChatMessageController(IChatMessageAppService messageAppService)
    {
        _messageAppService = messageAppService;
    }

    [HttpPost]
    [Authorize(ChatPermissions.Messages.Send)]
    public virtual Task<ChatMessageDto> SendAsync([FromBody] SendChatMessageInput input)
    {
        return _messageAppService.SendAsync(input);
    }

    [HttpGet]
    [Authorize(ChatPermissions.Messages.Default)]
    public virtual Task<PagedResultDto<ChatMessageDto>> GetListAsync([FromQuery] GetChatMessageListInput input)
    {
        return _messageAppService.GetListAsync(input);
    }

    [HttpDelete("{messageId}")]
    [Authorize(ChatPermissions.Messages.Delete)]
    public virtual Task DeleteAsync(Guid messageId)
    {
        return _messageAppService.DeleteAsync(messageId);
    }
}
