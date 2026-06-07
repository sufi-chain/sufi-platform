using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Features;
using SufiChain.SufiAbp.Features;

namespace SufiChain.Chat.Blazor.Toolbars;

/// <summary>
/// Renders a chat inbox toggle button shown next to language and theme switches
/// in the KomTheme icon rail footer. Visibility is gated by the Chat.Enable feature
/// and the Chat.Inbox.User permission (handled via AuthorizeView).
/// </summary>
public partial class ChatInboxToolbarComponent : ChatComponentBase
{
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected IFeatureChecker FeatureChecker => LazyGetRequiredService(ref _featureChecker);
    private IFeatureChecker? _featureChecker;

    protected bool IsVisible { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        IsVisible = await FeatureChecker.IsEnabledAsync(ChatFeatures.Enable);
    }

    protected virtual void OpenInbox()
    {
        NavigationManager.NavigateTo("/chat/inbox");
    }
}
