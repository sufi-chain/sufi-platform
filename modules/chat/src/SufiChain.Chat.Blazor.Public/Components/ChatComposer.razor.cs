using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using SufiChain.Chat.Blazor.Public.Services;
using SufiChain.Chat.Composer;
using SufiChain.Chat.Messages;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatComposer : ChatPublicComponentBase, IAsyncDisposable
{
    protected IChatComposerUploadAppService UploadAppService => LazyGetRequiredService(ref _uploadAppService);
    private IChatComposerUploadAppService? _uploadAppService;

    // IMPORTANT: Inject via Blazor [Inject] (renderer/circuit scope) — NOT LazyGetRequiredService.
    // SufiAbpComponentBase is an OwningComponentBase; its ScopedServices is a child scope whose
    // IJSRuntime is NOT attached to the live SignalR circuit, so JS interop would throw
    // "JavaScript interop calls cannot be issued ... statically rendered". Blazor [Inject] resolves
    // ChatComposerJsInterop from the circuit scope, giving the live RemoteJSRuntime.
    [Inject]
    protected ChatComposerJsInterop ComposerJs { get; set; } = default!;

    [Parameter]
    public Guid? SessionId { get; set; }

    [Parameter]
    public ChatComposerCapabilitiesDto? Capabilities { get; set; }

    [Parameter]
    public string DraftMessage { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> DraftMessageChanged { get; set; }

    [Parameter]
    public List<Guid> DraftAttachmentFileIds { get; set; } = new();

    [Parameter]
    public EventCallback<List<Guid>> DraftAttachmentFileIdsChanged { get; set; }

    [Parameter]
    public string? DraftMetadataJson { get; set; }

    [Parameter]
    public EventCallback<string?> DraftMetadataJsonChanged { get; set; }

    [Parameter]
    public EventCallback<ChatComposerSendRequest> OnSend { get; set; }

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public bool IsSignupRequired { get; set; }

    [Parameter]
    public string? SignupRequiredLocalizationKey { get; set; }

    [Parameter]
    public string? SignInUrl { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment? ComposerToolbar { get; set; }

    private ElementReference _shellRef;
    private ElementReference _textAreaRef;
    private ElementReference _photoInputRef;
    private ElementReference _documentInputRef;
    private readonly string _photoInputId = $"chat-composer-photo-{Guid.NewGuid():N}";
    private readonly string _documentInputId = $"chat-composer-document-{Guid.NewGuid():N}";
    private string _draft = string.Empty;
    private bool _isBusy;
    private bool _isRecordingVoice;
    private bool _isInteractiveRendered;
    private readonly List<ChatComposerPendingItem> _pendingItems = new();

    protected bool ShowRichActions => Capabilities?.CanUseRichComposer == true;

    protected bool HasPendingContent => _pendingItems.Count > 0;

    protected bool CanSend =>
        !string.IsNullOrWhiteSpace(_draft) || _pendingItems.Any(item => item.Kind != ChatComposerPendingKind.Text);

    protected override void OnParametersSet()
    {
        _draft = DraftMessage;
        SyncPendingItemsFromParameters();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isInteractiveRendered = true;
        }
    }

    protected string GetSignupRequiredMessage()
    {
        var key = string.IsNullOrWhiteSpace(SignupRequiredLocalizationKey)
            ? "Chat:AuthenticationRequired"
            : SignupRequiredLocalizationKey;

        return L[key];
    }

    protected async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (args.Key == "Enter" && !args.ShiftKey && CanSend && !IsDisabled)
        {
            await SendAsync();
        }
    }

    protected async Task InsertEmojiAsync(string emoji)
    {
        // Simple append - no cursor position management needed
        _draft += emoji;
        await DraftMessageChanged.InvokeAsync(_draft);
    }

    protected async Task ShareLocationAsync()
    {
        Logger.LogDebug("[ChatComposer] ShareLocationAsync invoked (SessionId={SessionId}, CanShareLocation={CanShare})",
            SessionId, Capabilities?.CanShareLocation);

        if (SessionId == null || Capabilities?.CanShareLocation != true)
        {
            Logger.LogDebug("[ChatComposer] ShareLocationAsync skipped — no session or capability disabled");
            return;
        }

        _isBusy = true;
        try
        {
            var location = await ComposerJs.GetGeolocationAsync();
            var label = L["Composer:LocationShared"];
            var metadataJson = ChatMessageMetadata.BuildLocationJson(
                location.Latitude,
                location.Longitude,
                location.AccuracyMeters,
                label);

            DraftMetadataJson = metadataJson;
            await DraftMetadataJsonChanged.InvokeAsync(metadataJson);

            _pendingItems.RemoveAll(item => item.Kind == ChatComposerPendingKind.Location);
            _pendingItems.Add(new ChatComposerPendingItem
            {
                Kind = ChatComposerPendingKind.Location,
                Label = label
            });
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains("JavaScript module not available"))
        {
            // Not ready yet, silently ignore
        }
        catch (JSException exception)
        {
            await Message.ErrorAsync(exception.Message);
        }
        finally
        {
            _isBusy = false;
        }
    }

    protected async Task OnPhotoInputChangedAsync(ChangeEventArgs args)
    {
        Logger.LogDebug("[ChatComposer] OnPhotoInputChangedAsync fired (file input changed)");
        await UploadFromInputAsync(_photoInputRef, "photo");
    }

    protected async Task OnDocumentInputChangedAsync(ChangeEventArgs args)
    {
        Logger.LogDebug("[ChatComposer] OnDocumentInputChangedAsync fired (file input changed)");
        await UploadFromInputAsync(_documentInputRef, "document");
    }

    protected async Task StartVoiceRecordingAsync()
    {
        Logger.LogDebug("[ChatComposer] StartVoiceRecordingAsync invoked (CanRecordVoice={CanRecord})", Capabilities?.CanRecordVoice);

        if (Capabilities?.CanRecordVoice != true)
        {
            Logger.LogDebug("[ChatComposer] StartVoiceRecordingAsync skipped — voice capability disabled");
            return;
        }

        try
        {
            await ComposerJs.StartVoiceRecordingAsync();
            _isRecordingVoice = true;
            Logger.LogDebug("[ChatComposer] StartVoiceRecordingAsync — recording started");
        }
        catch (InvalidOperationException exception)
        {
            // Module not loaded yet, silently ignore
            if (exception.Message.Contains("JavaScript module not available"))
            {
                Logger.LogWarning("[ChatComposer] StartVoiceRecordingAsync — JS module not available yet");
                return;
            }
            Logger.LogError(exception, "[ChatComposer] StartVoiceRecordingAsync failed");
            await Message.ErrorAsync(exception.Message);
        }
        catch (JSException exception)
        {
            Logger.LogError(exception, "[ChatComposer] StartVoiceRecordingAsync JS error");
            await Message.ErrorAsync(exception.Message);
        }
    }

    protected async Task StopVoiceRecordingAsync()
    {
        Logger.LogDebug("[ChatComposer] StopVoiceRecordingAsync invoked (SessionId={SessionId})", SessionId);

        if (SessionId == null)
        {
            Logger.LogDebug("[ChatComposer] StopVoiceRecordingAsync skipped — no session");
            return;
        }

        _isBusy = true;
        try
        {
            var recording = await ComposerJs.StopVoiceRecordingAsync();
            _isRecordingVoice = false;
            Logger.LogDebug("[ChatComposer] StopVoiceRecordingAsync — recording size={Size} bytes", recording.Size);

            if (recording.Size == 0)
            {
                Logger.LogDebug("[ChatComposer] StopVoiceRecordingAsync — empty recording, nothing to upload");
                return;
            }

            var base64 = recording.Base64;
            var mimeType = recording.MimeType;
            var extension = mimeType.Contains("webm", StringComparison.OrdinalIgnoreCase) ? "webm" : "ogg";
            var fileName = $"voice_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{extension}";

            // Convert base64 to bytes
            var bytes = Convert.FromBase64String(base64);

            var result = await UploadAppService.UploadAsync(new ChatComposerUploadInput
            {
                SessionId = SessionId.Value,
                FileName = fileName,
                MimeType = mimeType,
                Content = bytes,
                IsVoiceRecording = true
            });

            DraftAttachmentFileIds.Add(result.Id);
            await DraftAttachmentFileIdsChanged.InvokeAsync(DraftAttachmentFileIds);

            var durationSeconds = (int)(recording.Size / 16000.0);
            var metadataJson = ChatMessageMetadata.BuildVoiceJson(durationSeconds, mimeType);

            DraftMetadataJson = metadataJson;
            await DraftMetadataJsonChanged.InvokeAsync(metadataJson);

            _pendingItems.Add(new ChatComposerPendingItem
            {
                Kind = ChatComposerPendingKind.Voice,
                FileId = result.Id,
                Label = $"{durationSeconds}s"
            });
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains("JavaScript module not available"))
        {
            _isRecordingVoice = false;
            Logger.LogWarning("[ChatComposer] StopVoiceRecordingAsync — JS module not available yet");
        }
        catch (JSException exception)
        {
            _isRecordingVoice = false;
            Logger.LogError(exception, "[ChatComposer] StopVoiceRecordingAsync JS error");
            await HandleErrorAsync(exception);
        }
        catch (Exception exception)
        {
            _isRecordingVoice = false;
            Logger.LogError(exception, "[ChatComposer] StopVoiceRecordingAsync failed");
            await HandleErrorAsync(exception);
        }
        finally
        {
            _isBusy = false;
        }
    }

    protected async Task RemovePendingItemAsync(ChatComposerPendingItem item)
    {
        _pendingItems.Remove(item);

        if (item.FileId.HasValue)
        {
            DraftAttachmentFileIds.Remove(item.FileId.Value);
            await DraftAttachmentFileIdsChanged.InvokeAsync(DraftAttachmentFileIds);
        }

        if (item.Kind == ChatComposerPendingKind.Location || item.Kind == ChatComposerPendingKind.Voice)
        {
            DraftMetadataJson = null;
            await DraftMetadataJsonChanged.InvokeAsync(null);
        }
    }

    protected async Task SendAsync()
    {
        if (!CanSend || IsDisabled)
        {
            return;
        }

        var request = new ChatComposerSendRequest
        {
            Body = _draft.Trim(),
            AttachmentFileIds = new List<Guid>(DraftAttachmentFileIds),
            MetadataJson = DraftMetadataJson
        };

        _draft = string.Empty;
        _pendingItems.Clear();
        DraftAttachmentFileIds.Clear();
        DraftMetadataJson = null;

        await DraftMessageChanged.InvokeAsync(_draft);
        await DraftAttachmentFileIdsChanged.InvokeAsync(DraftAttachmentFileIds);
        await DraftMetadataJsonChanged.InvokeAsync(null);

        await OnSend.InvokeAsync(request);
    }

    protected virtual async Task UploadFromInputAsync(ElementReference inputRef, string kind)
    {
        Logger.LogDebug("[ChatComposer] UploadFromInputAsync invoked (kind={Kind}, SessionId={SessionId})", kind, SessionId);

        if (SessionId == null)
        {
            Logger.LogDebug("[ChatComposer] UploadFromInputAsync skipped — no session");
            return;
        }

        _isBusy = true;
        try
        {
            var files = await ComposerJs.ReadInputFilesAsync(inputRef);
            Logger.LogDebug("[ChatComposer] UploadFromInputAsync read {Count} file(s) from input", files.Length);
            if (files.Length == 0)
            {
                return;
            }

            foreach (var file in files)
            {
                // Convert base64 to bytes
                var bytes = Convert.FromBase64String(file.Base64);

                var result = await UploadAppService.UploadAsync(new ChatComposerUploadInput
                {
                    SessionId = SessionId.Value,
                    FileName = file.Name,
                    MimeType = file.Type,
                    Content = bytes,
                    IsVoiceRecording = false
                });

                DraftAttachmentFileIds.Add(result.Id);
                _pendingItems.Add(new ChatComposerPendingItem
                {
                    Kind = kind == "photo" ? ChatComposerPendingKind.Photo : ChatComposerPendingKind.Document,
                    FileId = result.Id,
                    Label = file.Name
                });
            }

            await DraftAttachmentFileIdsChanged.InvokeAsync(DraftAttachmentFileIds);
            Logger.LogDebug("[ChatComposer] UploadFromInputAsync completed (kind={Kind})", kind);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "[ChatComposer] UploadFromInputAsync failed (kind={Kind})", kind);
            await HandleErrorAsync(exception);
        }
        finally
        {
            _isBusy = false;
        }
    }

    protected virtual Task HandleErrorAsync(Exception exception)
    {
        Logger.LogError(exception, "[ChatComposer] HandleErrorAsync surfacing error to user: {Message}", exception.Message);
        return Message.ErrorAsync(exception.Message);
    }

    protected void SyncPendingItemsFromParameters()
    {
        if (_pendingItems.Count > 0)
        {
            return;
        }

        foreach (var fileId in DraftAttachmentFileIds)
        {
            _pendingItems.Add(new ChatComposerPendingItem
            {
                Kind = ChatComposerPendingKind.Document,
                FileId = fileId,
                Label = fileId.ToString("N")[..8]
            });
        }

        if (!string.IsNullOrWhiteSpace(DraftMetadataJson))
        {
            var metadata = ChatMessageMetadata.TryParse(DraftMetadataJson);
            if (metadata?.ContentKind == ChatMessageContentKind.Location)
            {
                _pendingItems.Add(new ChatComposerPendingItem
                {
                    Kind = ChatComposerPendingKind.Location,
                    Label = metadata.Location?.Label ?? L["Composer:LocationShared"]
                });
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (ComposerJs != null)
        {
            await ComposerJs.DisposeAsync();
        }
    }
}

public enum ChatComposerPendingKind
{
    Text,
    Photo,
    Document,
    Location,
    Voice
}

public sealed class ChatComposerPendingItem
{
    public ChatComposerPendingKind Kind { get; set; }

    public Guid? FileId { get; set; }

    public string Label { get; set; } = string.Empty;
}
