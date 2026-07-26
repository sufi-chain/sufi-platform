using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;
using SufiChain.SufiPlatform.FileManager.FileItems;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Components.Upload;

/// <summary>
/// Public file uploader. Uses HTTP multipart upload (not Blazor InputFile/circuit streams)
/// so large files do not hit SignalR "Did not receive any data in the allotted time".
/// </summary>
public partial class PublicFileUploader
{
    [Inject]
    protected PublicFileUploadJsInterop JsInterop { get; set; } = default!;

    [Parameter]
    public string? StructureKey { get; set; }

    [Parameter]
    public string? EntityType { get; set; }

    [Parameter]
    public Guid? EntityId { get; set; }

    [Parameter]
    public string? FolderPath { get; set; }

    [Parameter]
    public bool AutoConfirm { get; set; } = true;

    [Parameter]
    public bool AllowMultiple { get; set; } = true;

    [Parameter]
    public long MaxFileSize { get; set; } = 100 * 1024 * 1024;

    [Parameter]
    public int MaxFiles { get; set; } = 20;

    [Parameter]
    public string Accept { get; set; } = "image/*,application/pdf,video/*,.docx,.html,.htm,.txt,.md";

    [Parameter]
    public string Title { get; set; } = "Upload files";

    [Parameter]
    public string Description { get; set; } = "Drag files here or browse from your device.";

    [Parameter]
    public string BrowseButtonText { get; set; } = "Browse files";

    [Parameter]
    public string RemoveButtonText { get; set; } = "Remove file";

    /// <summary>
    /// When false, only the hidden file input and progress list are rendered (for immediate picker UX).
    /// </summary>
    [Parameter]
    public bool ShowDropZone { get; set; } = true;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public EventCallback<List<FileItemDto>> Uploaded { get; set; }

    [Parameter]
    public EventCallback<string> UploadFailed { get; set; }

    private readonly string _fileInputId = $"public-file-uploader-{Guid.NewGuid():N}";
    private readonly List<SelectedUploadFile> _selectedFiles = new();
    private readonly List<string> _errors = new();
    private readonly List<FileItemDto> _batchUploaded = new();
    private bool _isDragging;
    private bool _disposed;
    private bool _changeHandlerRegistered;
    private bool _pickerReady;
    private DotNetObjectReference<PublicFileUploader>? _dotNetRef;
    private int _expectedCompletions;
    private int _receivedCompletions;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await EnsureChangeHandlerRegisteredAsync();
        }
    }

    /// <summary>
    /// Native file input id (for optional &lt;label for&gt; wiring from a parent).
    /// </summary>
    public string FileInputId => _fileInputId;

    /// <summary>
    /// Opens the native file explorer immediately (no intermediate upload dialog).
    /// Must be called from a user gesture (e.g. button click) without prior delays.
    /// </summary>
    public async Task OpenFilePickerAsync()
    {
        if (_disposed)
        {
            return;
        }

        await EnsureChangeHandlerRegisteredAsync();

        if (_disposed || !_pickerReady)
        {
            return;
        }

        await JsInterop.TriggerFileInputAsync(_fileInputId);
    }

    private async Task EnsureChangeHandlerRegisteredAsync()
    {
        if (_changeHandlerRegistered || _disposed)
        {
            return;
        }

        _changeHandlerRegistered = true;
        _dotNetRef = DotNetObjectReference.Create(this);
        await JsInterop.RegisterFileInputChangeAsync(_fileInputId, _dotNetRef);
        _pickerReady = true;
    }

    protected virtual void HandleDragEnter()
    {
        _isDragging = true;
    }

    protected virtual void HandleDragLeave()
    {
        _isDragging = false;
    }

    protected virtual Task HandleDrop(DragEventArgs args)
    {
        _isDragging = false;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called by JS when the file input changes. Starts HTTP upload (bypasses SignalR).
    /// </summary>
    [JSInvokable]
    public async Task OnFileInputChange()
    {
        if (_disposed || _dotNetRef == null)
        {
            return;
        }

        var metadata = new PublicJsUploadMetadata
        {
            StructureKey = StructureKey,
            EntityType = EntityType,
            EntityId = EntityId,
            FolderPath = string.IsNullOrWhiteSpace(FolderPath) ? null : FolderPath.Trim(),
            AutoConfirm = AutoConfirm,
            MaxFileSize = MaxFileSize
        };

        await JsInterop.UploadFilesFromInputAsync(_fileInputId, metadata, accessToken: null, _dotNetRef);
    }

    /// <summary>
    /// Called by JS with selected file infos before upload starts.
    /// </summary>
    [JSInvokable]
    public void OnFilesSelected(PublicJsFileInfo[] fileInfos)
    {
        if (_disposed || fileInfos == null)
        {
            return;
        }

        _errors.Clear();
        _selectedFiles.Clear();
        _batchUploaded.Clear();
        _receivedCompletions = 0;

        var limited = fileInfos.Take(AllowMultiple ? MaxFiles : 1).ToArray();
        _expectedCompletions = limited.Length;

        if (fileInfos.Length > limited.Length)
        {
            _errors.Add($"Only the first {limited.Length} file(s) will be uploaded.");
        }

        foreach (var info in limited)
        {
            var selected = new SelectedUploadFile(info.Name, info.Size, info.Type);
            if (info.Size > MaxFileSize)
            {
                selected.HasError = true;
                selected.ErrorMessage = $"File '{info.Name}' exceeds the maximum size of {FormatFileSize(MaxFileSize)}.";
                _errors.Add(selected.ErrorMessage);
            }
            else
            {
                selected.IsUploading = true;
                selected.Progress = 0;
            }

            _selectedFiles.Add(selected);
        }

        _ = InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public void OnUploadProgress(int fileIndex, int progress)
    {
        if (_disposed || fileIndex < 0 || fileIndex >= _selectedFiles.Count)
        {
            return;
        }

        var file = _selectedFiles[fileIndex];
        if (file.HasError)
        {
            return;
        }

        file.Progress = progress;
        _ = InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public async Task OnUploadComplete(int fileIndex, PublicJsUploadResult result)
    {
        if (_disposed || fileIndex < 0 || fileIndex >= _selectedFiles.Count)
        {
            return;
        }

        var selectedFile = _selectedFiles[fileIndex];
        selectedFile.IsUploading = false;
        _receivedCompletions++;

        if (result.Success && result.Data.HasValue)
        {
            try
            {
                var dto = result.Data.Value.Deserialize<FileItemDto>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (dto != null)
                {
                    selectedFile.Progress = 100;
                    selectedFile.IsUploaded = true;
                    selectedFile.FileItem = dto;
                    _batchUploaded.Add(dto);
                }
            }
            catch (JsonException ex)
            {
                selectedFile.HasError = true;
                selectedFile.ErrorMessage = ex.Message;
                _errors.Add(ex.Message);
                await UploadFailed.InvokeAsync(ex.Message);
            }
        }
        else
        {
            var error = result.Error ?? selectedFile.ErrorMessage;
            if (string.IsNullOrWhiteSpace(error))
            {
                error = "Upload failed.";
            }

            selectedFile.HasError = true;
            selectedFile.ErrorMessage = error;
            if (!_errors.Contains(error))
            {
                _errors.Add(error);
            }

            await UploadFailed.InvokeAsync(error);
        }

        await InvokeAsync(StateHasChanged);

        if (_receivedCompletions >= _expectedCompletions && _batchUploaded.Count > 0)
        {
            var uploaded = _batchUploaded.ToList();
            await Uploaded.InvokeAsync(uploaded);

            // Compact / immediate-picker mode: clear progress after parent receives files.
            if (!ShowDropZone)
            {
                _selectedFiles.Clear();
                _errors.Clear();
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    protected virtual void RemoveFile(SelectedUploadFile file)
    {
        _selectedFiles.Remove(file);
    }

    protected virtual string GetFileIcon(SelectedUploadFile file)
    {
        if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return "image";
        }

        if (file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            return "video";
        }

        if (file.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
        {
            return "file-text";
        }

        return "file";
    }

    protected virtual string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024d:0.#} KB";
        }

        return $"{bytes / 1024d / 1024d:0.#} MB";
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }

    protected sealed class SelectedUploadFile
    {
        public SelectedUploadFile(string name, long size, string contentType)
        {
            Name = name;
            Size = size;
            ContentType = contentType ?? string.Empty;
        }

        public string Name { get; }
        public long Size { get; }
        public string ContentType { get; }
        public int Progress { get; set; }
        public bool IsUploading { get; set; }
        public bool IsUploaded { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public FileItemDto? FileItem { get; set; }
    }
}
