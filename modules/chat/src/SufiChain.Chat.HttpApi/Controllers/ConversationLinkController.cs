using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Permissions;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/conversation-links")]
public class ConversationLinkController : ChatController, IConversationLinkAppService
{
    private readonly IConversationLinkAppService _conversationLinkAppService;

    public ConversationLinkController(IConversationLinkAppService conversationLinkAppService)
    {
        _conversationLinkAppService = conversationLinkAppService;
    }

    [HttpPost]
    [Authorize(ChatPermissions.Links.Manage)]
    public virtual Task<ConversationLinkDto> CreateAsync([FromBody] CreateConversationLinkInput input)
    {
        return _conversationLinkAppService.CreateAsync(input);
    }

    [HttpGet("by-session/{sessionId}")]
    [Authorize(ChatPermissions.Links.Default)]
    public virtual Task<List<ConversationLinkDto>> GetBySessionAsync(Guid sessionId)
    {
        return _conversationLinkAppService.GetBySessionAsync(sessionId);
    }

    [HttpGet("by-entity/{linkedEntityType}/{linkedEntityId}")]
    [Authorize(ChatPermissions.Links.Default)]
    public virtual Task<List<ConversationLinkDto>> GetByEntityAsync(string linkedEntityType, Guid linkedEntityId)
    {
        return _conversationLinkAppService.GetByEntityAsync(linkedEntityType, linkedEntityId);
    }

    [HttpDelete("{linkId}")]
    [Authorize(ChatPermissions.Links.Manage)]
    public virtual Task DeleteAsync(Guid linkId)
    {
        return _conversationLinkAppService.DeleteAsync(linkId);
    }
}
