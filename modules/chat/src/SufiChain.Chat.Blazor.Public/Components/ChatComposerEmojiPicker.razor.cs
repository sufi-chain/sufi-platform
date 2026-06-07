using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SufiChain.Chat.Blazor.Public.Services;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatComposerEmojiPicker : ChatPublicComponentBase, IAsyncDisposable
{
    [Inject]
    protected ChatComposerJsInterop ComposerJs { get; set; } = default!;

    [Parameter]
    public ElementReference ShellRef { get; set; }

    [Parameter]
    public EventCallback<string> OnEmojiSelected { get; set; }

    private ElementReference _anchorRef;
    private ElementReference _popoverRef;
    private DotNetObjectReference<ChatComposerEmojiPicker>? _dotNetRef;
    private bool _isOpen;

    private static readonly string[] _emojis =
    [
        "😀", "😁", "😂", "🤣", "😊", "😍", "😘", "😎", "🤔", "👍",
        "🙏", "👏", "🔥", "✅", "❤️", "🎉", "😢", "😡", "🙌", "💡"
    ];

    protected async Task ToggleAsync()
    {
        _isOpen = !_isOpen;
        await InvokeAsync(StateHasChanged);
    }

    protected async Task SelectAsync(string emoji)
    {
        _isOpen = false;
        await OnEmojiSelected.InvokeAsync(emoji);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
        }

        await UpdatePopoverAsync();
    }

    private async Task UpdatePopoverAsync()
    {
        if (!_isOpen)
        {
            await ComposerJs.UnregisterPopoverClickAwayAsync(_anchorRef);
            return;
        }

        await ComposerJs.PositionOverlayPopoverAsync(_anchorRef, _popoverRef, ShellRef, "shell");
        if (_dotNetRef != null)
        {
            await ComposerJs.RegisterPopoverClickAwayAsync(_anchorRef, _dotNetRef);
        }
    }

    [JSInvokable]
    public async Task OnPopoverClickAway()
    {
        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        await ComposerJs.UnregisterPopoverClickAwayAsync(_anchorRef);
        _dotNetRef?.Dispose();
    }
}
