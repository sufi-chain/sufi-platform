using SufiChain.Chat.Blazor.Public.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.Chat.Blazor;

/// <summary>
/// Base class for Chat Blazor pages and components.
/// </summary>
public abstract class ChatComponentBase : SufiAbpComponentBase
{
    protected ChatComponentBase()
    {
        LocalizationResource = typeof(ChatPublicResource);
    }
}
