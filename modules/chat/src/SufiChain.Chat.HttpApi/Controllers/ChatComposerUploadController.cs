using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.Composer;
using SufiChain.Chat.Permissions;
using SufiChain.SufiAbp.FileManager.FileItems;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/composer/upload")]
public class ChatComposerUploadController : ChatController, IChatComposerUploadAppService
{
    private readonly IChatComposerUploadAppService _uploadAppService;

    public ChatComposerUploadController(IChatComposerUploadAppService uploadAppService)
    {
        _uploadAppService = uploadAppService;
    }

    [HttpPost]
    [Authorize(ChatPermissions.Messages.Send)]
    public virtual Task<FileItemDto> UploadAsync([FromBody] ChatComposerUploadInput input)
    {
        return _uploadAppService.UploadAsync(input);
    }
}
