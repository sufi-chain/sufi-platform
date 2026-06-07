using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Composer;
using SufiChain.SufiAbp.FileManager.Blazor.Components.Gallery;
using SufiChain.SufiAbp.FileManager.FileItems;

namespace SufiChain.Chat.Blazor.Components;

public partial class ChatOperatorComposerExtensions : ChatComponentBase
{
    protected IChatOperatorCopilotAppService CopilotAppService => LazyGetRequiredService(ref _copilotAppService);
    private IChatOperatorCopilotAppService? _copilotAppService;

    [Parameter]
    public Guid? SessionId { get; set; }

    [Parameter]
    public ChatComposerCapabilitiesDto? Capabilities { get; set; }

    [Parameter]
    public string DraftText { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> DraftTextChanged { get; set; }

    [Parameter]
    public EventCallback<IReadOnlyList<Guid>> AttachmentFileIdsChanged { get; set; }

    private FileSelector? _fileSelector;

    private bool _galleryOpen;

    private bool _copilotOpen;

    private ChatOperatorCopilotOperation _operation = ChatOperatorCopilotOperation.Rewrite;

    private string? _prompt;

    protected bool IsBusy { get; set; }

    protected Task ToggleGalleryAsync()
    {
        _galleryOpen = !_galleryOpen;
        return Task.CompletedTask;
    }

    protected Task ToggleCopilotAsync()
    {
        _copilotOpen = !_copilotOpen;
        return Task.CompletedTask;
    }

    protected async Task OpenGalleryAsync()
    {
        _galleryOpen = false;

        if (_fileSelector != null)
        {
            await _fileSelector.ShowAsync();
        }
    }

    protected async Task OnGalleryFilesSelectedAsync(List<FileItemDto> files)
    {
        if (files.Count == 0)
        {
            return;
        }

        await AttachmentFileIdsChanged.InvokeAsync(files.Select(file => file.Id).ToList());
        await Message.SuccessAsync(L["Composer:GallerySelected", files.Count]);
    }

    protected async Task RunCopilotAsync()
    {
        if (!SessionId.HasValue)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await CopilotAppService.AssistAsync(new ChatOperatorCopilotInput
            {
                SessionId = SessionId.Value,
                DraftText = DraftText,
                Prompt = _prompt,
                Operation = _operation
            });

            await DraftTextChanged.InvokeAsync(result.SuggestedText);
            await Message.SuccessAsync(L["Composer:AiCompleted"]);
            _copilotOpen = false;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
