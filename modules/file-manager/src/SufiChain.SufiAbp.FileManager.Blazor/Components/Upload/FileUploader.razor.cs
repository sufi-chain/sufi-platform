using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using SufiChain.SufiBlazor.Components.Feedback;
using SufiChain.SufiAbp.FileManager.Blazor.Services;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.FileManager.Blazor.Components.Upload;

/// <summary>
/// File uploader component with drag-and-drop support and progress tracking.
/// Uses direct HTTP upload (bypasses SignalR) to avoid circuit timeout with large files.
/// </summary>
public partial class FileUploader : FileManagerComponentBase, IDisposable
{

    [Inject]
    protected IFileItemAppService FileItemAppService { get; set; } = default!;

    [Inject]
    protected IFileStructureAppService FileStructureAppService { get; set; } = default!;

    [Inject]
    protected FileManagerJsInterop JsInterop { get; set; } = default!;

    [Inject]
    protected IConfiguration Configuration { get; set; } = default!;

    [Inject]
    protected IFileUploadAccessTokenProvider? AccessTokenProvider { get; set; }

    [Parameter] public string? StructureKey { get; set; }
    [Parameter] public string? EntityType { get; set; }
    [Parameter] public Guid? EntityId { get; set; }
    /// <summary>
    /// Target folder path (e.g. "/web/tourist"). When set, folders are created if missing.
    /// When not set, an optional input is shown for the user to type the path. Empty = root.
    /// </summary>
    [Parameter] public string? FolderPath { get; set; }
    [Parameter] public bool AutoConfirm { get; set; } = false;
    [Parameter] public bool AllowMultiple { get; set; } = true;
    [Parameter] public bool ShowStorageQuota { get; set; } = true;
    [Parameter] public bool ShowStructureSelector { get; set; } = false;
    /// <summary>
    /// When true and FolderPath is not passed, shows an input for the user to type the target folder path. Default false = upload to root.
    /// </summary>
    [Parameter] public bool ShowFolderPathField { get; set; } = false;
    [Parameter] public string DropZoneTitle { get; set; } = "Drag & Drop Files Here";
    [Parameter] public string DropZoneDescription { get; set; } = "or click Browse Files to select";
    [Parameter] public EventCallback<List<FileItemDto>> OnUploadCompleted { get; set; }
    [Parameter] public EventCallback<string> OnUploadError { get; set; }
    [Parameter] public string CssClass { get; set; } = "";


    private FileStructureDto? _structure;
    private StorageQuotaDto? _storageQuota;
    private bool _isDragging = false;
    private List<UploadingFile> _uploadingFiles = new();
    private List<FileItemDto> _uploadedFiles = new();
    private List<FileStructureDto> _availableStructures = new();
    private string? _selectedStructureKey;
    private string _userFolderPath = "";
    private bool _configModalOpen;
    private readonly string _fileInputId = $"fileUploader-fileInput-{Guid.NewGuid():N}";

    private bool _hasConfigOptions =>
        (ShowStructureSelector && _availableStructures.Any()) ||
        (ShowFolderPathField && string.IsNullOrEmpty(FolderPath));
    private bool _disposed = false;
    private DotNetObjectReference<FileUploader>? _dotNetRef;
    private bool _changeHandlerRegistered;

    protected override void OnInitialized()
    {
        // Set initial structure key - use provided key or default to "General"
        _selectedStructureKey = StructureKey ?? FileStructureKeys.General;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            // Register file input handler
            if (!_changeHandlerRegistered)
            {
                _changeHandlerRegistered = true;
                _dotNetRef = DotNetObjectReference.Create(this);
                await JsInterop.RegisterFileInputChangeAsync(_fileInputId, _dotNetRef);
            }

            // Load available structures if selector is shown
            if (ShowStructureSelector)
            {
                try
                {
                    var result = await FileStructureAppService.GetListAsync(new PagedAndSortedResultRequestDto
                    {
                        MaxResultCount = 100,
                        Sorting = "DisplayName"
                    });
                    _availableStructures = result.Items.ToList();
                }
                catch (Exception ex)
                {
                    await Notify.ErrorAsync(L["FailedToLoadStructures", ex.Message]);
                }
            }

            // Load structure
            await LoadStructureAsync(_selectedStructureKey);

            if (ShowStorageQuota)
            {
                try
                {
                    _storageQuota = await FileItemAppService.GetStorageQuotaAsync();
                }
                catch
                {
                    await Notify.WarnAsync(L["CouldNotLoadStorageQuota"]);
                }
            }

            StateHasChanged();
        }
    }

    private async Task LoadStructureAsync(string? structureKey)
    {
        if (string.IsNullOrEmpty(structureKey))
        {
            _structure = null;
            return;
        }

        try
        {
            _structure = await FileStructureAppService.GetByKeyAsync(structureKey);
            AllowMultiple = _structure.IsMultiple;
        }
        catch (Exception ex)
        {
            await Notify.ErrorAsync(L["FailedToLoadStructures", ex.Message]);
            _structure = null;
        }
    }

    private async Task OnStructureChanged(string? newStructureKey)
    {
        _selectedStructureKey = newStructureKey;
        await LoadStructureAsync(newStructureKey);
        StateHasChanged();
    }

    private void HandleDragEnter()
    {
        _isDragging = true;
    }

    private void HandleDragLeave()
    {
        _isDragging = false;
    }

    private async Task HandleDrop(DragEventArgs e)
    {
        _isDragging = false;
        // Note: File drop handling requires JavaScript interop
        await Task.CompletedTask;
    }

    /// <summary>
    /// Called by JS when the file input's change event fires. Starts HTTP upload (bypasses SignalR).
    /// </summary>
    [JSInvokable]
    public async Task OnFileInputChange()
    {
        if (_disposed) return;

        var accessToken = AccessTokenProvider != null ? await AccessTokenProvider.GetAccessTokenAsync() : null;
        var effectivePath = !string.IsNullOrEmpty(FolderPath)
            ? FolderPath.Trim()
            : (ShowFolderPathField && !string.IsNullOrWhiteSpace(_userFolderPath) ? _userFolderPath.Trim() : null);
        var metadata = new JsUploadMetadata
        {
            StructureKey = _selectedStructureKey ?? StructureKey,
            EntityType = EntityType,
            EntityId = EntityId,
            FolderPath = effectivePath,
            AutoConfirm = AutoConfirm,
            Alt = null
        };
        if (_dotNetRef != null)
            await JsInterop.UploadFilesFromInputAsync(_fileInputId, metadata, accessToken, _dotNetRef);
    }

    /// <summary>
    /// Called by JS with selected file infos before upload starts. Populates _uploadingFiles.
    /// </summary>
    [JSInvokable]
    public void OnFilesSelected(JsFileInfo[] fileInfos)
    {
        if (_disposed || fileInfos == null) return;
        var maxAllowedSize = _structure?.MaxFileSize ?? 100 * 1024 * 1024;
        foreach (var info in fileInfos)
        {
            var uploadingFile = new UploadingFile
            {
                FileName = info.Name,
                FileSize = info.Size,
                Progress = 0,
                IsCompleted = false,
                StatusMessage = L["Uploading"].Value!
            };
            if (info.Size > maxAllowedSize)
            {
                uploadingFile.HasError = true;
                uploadingFile.ErrorMessage = L["FileSizeExceedsMaximumDetail", FormatFileSize(info.Size), FormatFileSize(maxAllowedSize)].Value;
            }
            _uploadingFiles.Add(uploadingFile);
        }
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Called by JS with progress for a file (index, 0-100).
    /// </summary>
    [JSInvokable]
    public void OnUploadProgress(int fileIndex, int progress)
    {
        if (_disposed || fileIndex < 0 || fileIndex >= _uploadingFiles.Count) return;
        _uploadingFiles[fileIndex].Progress = progress;
        _uploadingFiles[fileIndex].StatusMessage = progress >= 100 ? L["UploadCompleted"].Value! : L["Uploading"].Value!;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Called by JS when a single file upload completes (success or error).
    /// </summary>
    [JSInvokable]
    public async Task OnUploadComplete(int fileIndex, JsUploadResult result)
    {
        if (_disposed || fileIndex < 0 || fileIndex >= _uploadingFiles.Count) return;
        var uploadingFile = _uploadingFiles[fileIndex];
        if (result.Success && result.Data.HasValue)
        {
            try
            {
                var dto = result.Data.Value.Deserialize<FileItemDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (dto != null)
                {
                    uploadingFile.Progress = 100;
                    uploadingFile.IsCompleted = true;
                    uploadingFile.StatusMessage = L["UploadCompleted"].Value!;
                    _uploadedFiles.Add(dto);
                    await OnUploadCompleted.InvokeAsync(new List<FileItemDto> { dto });
                }
            }
            catch (JsonException) { /* ignore */ }
        }
        else
        {
            uploadingFile.HasError = true;
            uploadingFile.Progress = 0;
            uploadingFile.ErrorMessage = result.Error ?? L["UploadFailed", ""].Value;
            uploadingFile.StatusMessage = uploadingFile.ErrorMessage;
            await Message.ErrorAsync(uploadingFile.ErrorMessage);
            await OnUploadError.InvokeAsync(uploadingFile.ErrorMessage);
        }

        if (uploadingFile.IsCompleted && !uploadingFile.HasError)
        {
            _ = RemoveUploadingFileAfterDelayAsync(uploadingFile);
        }
        await InvokeAsync(StateHasChanged);
        if (ShowStorageQuota)
        {
            try
            {
                _storageQuota = await FileItemAppService.GetStorageQuotaAsync();
            }
            catch { /* ignore */ }
        }
    }

    private void CancelUpload(UploadingFile file)
    {
        _uploadingFiles.Remove(file);
    }

    private string GetAcceptedFileTypes()
    {
        if (_structure == null)
            return "*/*";

        return _structure.AllowedExtensions.Replace(',', ' ');
    }

    private IEnumerable<string> GetAllowedExtensions()
    {
        if (_structure?.AllowedExtensions == null)
            return Array.Empty<string>();

        return _structure.AllowedExtensions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.TrimStart('.'))
            .Where(x => !string.IsNullOrEmpty(x));
    }

    private string FormatFileSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private async Task RemoveUploadingFileAfterDelayAsync(UploadingFile uploadingFile)
    {
        try
        {
            await Task.Delay(2000);
            if (!_disposed)
            {
                await InvokeAsync(() =>
                {
                    if (!_disposed)
                    {
                        _uploadingFiles.Remove(uploadingFile);
                        StateHasChanged();
                    }
                });
            }
        }
        catch (ObjectDisposedException)
        {
            // Component disposed, ignore
        }
    }



    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }

    private class UploadingFile
    {
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
        public int Progress { get; set; }
        public bool IsCompleted { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
        public string? StatusMessage { get; set; }
    }
}
