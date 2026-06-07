using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/sessions")]
public class ChatSessionController : ChatController, IChatSessionAppService
{
    private readonly IChatSessionAppService _sessionAppService;

    public ChatSessionController(IChatSessionAppService sessionAppService)
    {
        _sessionAppService = sessionAppService;
    }

    [HttpPost]
    [Authorize(ChatPermissions.Sessions.Create)]
    public virtual Task<ChatSessionDto> CreateAsync([FromBody] CreateChatSessionInput input)
    {
        return _sessionAppService.CreateAsync(input);
    }

    [HttpGet("{id}")]
    [Authorize(ChatPermissions.Sessions.Default)]
    public virtual Task<ChatSessionDto> GetAsync(Guid id)
    {
        return _sessionAppService.GetAsync(id);
    }

    [HttpGet]
    [Authorize(ChatPermissions.Sessions.Default)]
    public virtual Task<PagedResultDto<ChatSessionListDto>> GetListAsync([FromQuery] GetChatSessionListInput input)
    {
        return _sessionAppService.GetListAsync(input);
    }

    [HttpPost("{id}/close")]
    [Authorize(ChatPermissions.Sessions.Close)]
    public virtual Task CloseAsync(Guid id, [FromBody] CloseChatSessionInput? input = null)
    {
        return _sessionAppService.CloseAsync(id, input);
    }

    [HttpPost("{sessionId}/participants")]
    [Authorize(ChatPermissions.Sessions.Default)]
    public virtual Task AddParticipantAsync(Guid sessionId, [FromBody] AddChatParticipantInput input)
    {
        return _sessionAppService.AddParticipantAsync(sessionId, input);
    }

    [HttpDelete("{sessionId}/participants/{participantId}")]
    [Authorize(ChatPermissions.Sessions.Manage)]
    public virtual Task RemoveParticipantAsync(Guid sessionId, Guid participantId)
    {
        return _sessionAppService.RemoveParticipantAsync(sessionId, participantId);
    }

    [HttpGet("my")]
    [Authorize(ChatPermissions.Sessions.Default)]
    public virtual Task<PagedResultDto<ChatSessionListDto>> GetMySessionsAsync([FromQuery] GetMyChatSessionsInput input)
    {
        return _sessionAppService.GetMySessionsAsync(input);
    }

    [HttpPost("direct")]
    [Authorize(ChatPermissions.Sessions.Create)]
    public virtual Task<ChatSessionDto> GetOrCreateDirectSessionAsync([FromBody] GetOrCreateDirectSessionInput input)
    {
        return _sessionAppService.GetOrCreateDirectSessionAsync(input);
    }

    [HttpPost("groups")]
    [Authorize(ChatPermissions.Sessions.Create)]
    public virtual Task<ChatSessionDto> CreateGroupSessionAsync([FromBody] CreateGroupChatSessionInput input)
    {
        return _sessionAppService.CreateGroupSessionAsync(input);
    }

    [HttpPost("{sessionId}/join")]
    [Authorize(ChatPermissions.Sessions.Default)]
    public virtual Task<ChatSessionDto> JoinGroupSessionAsync(Guid sessionId)
    {
        return _sessionAppService.JoinGroupSessionAsync(sessionId);
    }

    [HttpPost("{sessionId}/leave")]
    [Authorize(ChatPermissions.Sessions.Default)]
    public virtual Task LeaveGroupSessionAsync(Guid sessionId)
    {
        return _sessionAppService.LeaveGroupSessionAsync(sessionId);
    }
}
