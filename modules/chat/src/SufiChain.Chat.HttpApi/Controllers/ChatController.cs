using SufiChain.Chat.Localization;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.Chat.Controllers;

public abstract class ChatController : SufiAbpControllerBase
{
    protected ChatController()
    {
        LocalizationResource = typeof(ChatResource);
    }
}
