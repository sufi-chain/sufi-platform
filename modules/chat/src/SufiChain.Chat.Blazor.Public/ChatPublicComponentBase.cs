using SufiChain.Chat.Blazor.Public.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.Chat.Blazor.Public;

/// <summary>
/// Base class for reusable public chat Blazor components.
/// </summary>
public abstract class ChatPublicComponentBase : SufiAbpComponentBase
{
    protected ChatPublicComponentBase()
    {
        LocalizationResource = typeof(ChatPublicResource);
    }
}
