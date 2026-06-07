using Microsoft.AspNetCore.Components;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatComposerAttachmentPreview : ChatPublicComponentBase
{
    [Parameter]
    public IReadOnlyList<ChatComposerPendingItem> Items { get; set; } = Array.Empty<ChatComposerPendingItem>();

    [Parameter]
    public EventCallback<ChatComposerPendingItem> OnRemove { get; set; }
}
