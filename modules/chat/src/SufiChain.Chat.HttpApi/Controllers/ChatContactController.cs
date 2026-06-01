using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.Contacts;
using SufiChain.Chat.Permissions;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/contacts")]
public class ChatContactController : ChatController, IChatContactAppService
{
    private readonly IChatContactAppService _contactAppService;

    public ChatContactController(IChatContactAppService contactAppService)
    {
        _contactAppService = contactAppService;
    }

    [HttpGet]
    [Authorize(ChatPermissions.Inbox.User)]
    public virtual Task<PagedResultDto<ChatContactDto>> SearchAsync([FromQuery] SearchChatContactsInput input)
    {
        return _contactAppService.SearchAsync(input);
    }
}
