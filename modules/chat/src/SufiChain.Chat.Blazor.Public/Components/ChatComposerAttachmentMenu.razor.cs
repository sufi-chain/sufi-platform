using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using SufiChain.Chat.Blazor.Public.Services;
using SufiChain.Chat.Composer;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatComposerAttachmentMenu : ChatPublicComponentBase, IAsyncDisposable
{
    [Inject]
    protected ChatComposerJsInterop ComposerJs { get; set; } = default!;

    [Parameter]
    public ElementReference ShellRef { get; set; }

    [Parameter]
    public ChatComposerCapabilitiesDto? Capabilities { get; set; }

    [Parameter]
    public Guid? SessionId { get; set; }

    [Parameter]
    public string? PhotoInputId { get; set; }

    [Parameter]
    public string? DocumentInputId { get; set; }

    [Parameter]
    public EventCallback OnLocationRequested { get; set; }

    private ElementReference _anchorRef;
    private ElementReference _popoverRef;
    private DotNetObjectReference<ChatComposerAttachmentMenu>? _dotNetRef;
    private bool _isOpen;

    protected async Task ToggleMenuAsync()
    {
        _isOpen = !_isOpen;
        Logger.LogDebug("[ChatComposer] AttachmentMenu ToggleMenuAsync — isOpen={IsOpen} (photoInputId={PhotoId}, documentInputId={DocId})", _isOpen, PhotoInputId, DocumentInputId);
        await InvokeAsync(StateHasChanged);
    }

    protected async Task CloseMenuAsync()
    {
        Logger.LogDebug("[ChatComposer] AttachmentMenu CloseMenuAsync (file label clicked → native file dialog should open)");
        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;
        await InvokeAsync(StateHasChanged);
    }

    protected async Task SelectLocationAsync()
    {
        Logger.LogDebug("[ChatComposer] AttachmentMenu SelectLocationAsync");
        _isOpen = false;
        await OnLocationRequested.InvokeAsync();
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

        await ComposerJs.PositionOverlayPopoverAsync(_anchorRef, _popoverRef, ShellRef, "anchor");
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
