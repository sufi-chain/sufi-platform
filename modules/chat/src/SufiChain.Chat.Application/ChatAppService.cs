using SufiChain.Chat.Localization;
using Volo.Abp.Application.Services;

namespace SufiChain.Chat;

public abstract class ChatAppService : ApplicationService
{
    protected ChatAppService()
    {
        LocalizationResource = typeof(ChatResource);
    }
}
