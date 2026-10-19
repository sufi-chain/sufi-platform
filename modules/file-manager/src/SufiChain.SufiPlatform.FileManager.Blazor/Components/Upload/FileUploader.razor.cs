using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using SufiChain.SufiBlazor.Components.Feedback;
using SufiChain.SufiPlatform.FileManager.Blazor.Services;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.FileManager.Uploads;
using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Components.Upload;

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
    [Parameter] public string? DropZoneTitle { get; set; }
    [Parameter] public string? DropZoneDescription { get; set; }
    [Parameter] public EventCallback<List<FileItemDto>> OnUploadCompleted { get; set; }
    [Parameter] public EventCallback<string> OnUploadError { get; set; }
    [Parameter] public string CssClass { get; set; } = "";


    private FileStructureDto? _structure;
    private StorageQuotaDto? _storageQuota;
    private bool _isDragging = false;
    private readonly UploadRowTracker<UploadingFile> _rows = new();
    private IReadOnlyList<UploadingFile> _uploadingFiles => _rows.Rows;
    private List<FileItemDto> _uploadedFiles = new();
    private List<FileStructureDto> _availableStructures = new();
    private readonly UploaderStructureKeyState _structureKeyState = new();
    private string? _selectedStructureKey => _structureKeyState.Selected;
    private bool _structureLoaded;
    private string _userFolderPath = "";
    private bool _settingsOpen;
    private readonly string _fileInputId = $"fileUploader-fileInput-{Guid.NewGuid():N}";

    private bool _hasConfigOptions =>
        (ShowStructureSelector && _availableStructures.Any()) ||
        (ShowFolderPathField && string.IsNullOrEmpty(FolderPath));
    private bool _disposed = false;
    private DotNetObjectReference<FileUploader>? _dotNetRef;
    private bool _changeHandlerRegistered;

    protected override async Task OnParametersSetAsync()
    {
        // sufi-chain#479: follow the parent's StructureKey on every render, not only the first; the File Manager
        // keeps this uploader mounted and changes the key as the user moves between structure folders.
        if (_structureKeyState.ApplyParameter(StructureKey) && _structureLoaded)
        {
            await LoadStructureAsync(_selectedStructureKey);
        }
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
                    await NotifyOperationFailedAsync(ex, "FailedToLoadStructures");
                }
            }

            // Load structure
            await LoadStructureAsync(_selectedStructureKey);
            _structureLoaded = true;

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
            await NotifyOperationFailedAsync(ex, "FailedToLoadStructures");
            _structure = null;
        }
    }

    private async Task OnStructureChanged(string? newStructureKey)
    {
        _structureKeyState.Select(newStructureKey);
        await LoadStructureAsync(_selectedStructureKey);
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
    /// <returns>The id of the batch's first row; JS reports each file as that id plus its position (sufi-chain#479).</returns>
    [JSInvokable]
    public int OnFilesSelected(JsFileInfo[] fileInfos)
    {
        if (_disposed || fileInfos == null) return -1;
        var maxAllowedSize = _structure?.MaxFileSize ?? 100 * 1024 * 1024;
        var batch = new List<UploadingFile>();
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
            batch.Add(uploadingFile);
        }
        var firstRowId = _rows.AddBatch(batch);
        InvokeAsync(StateHasChanged);
        return firstRowId;
    }

    /// <summary>
    /// Called by JS with progress for a file (index, 0-100).
    /// </summary>
    [JSInvokable]
    public void OnUploadProgress(int rowId, int progress)
    {
        if (_disposed) return;
        var file = _rows.Find(rowId);
        if (file == null) return;
        // sufi-chain#479: progress events are fire-and-forget and can arrive after the result. Once a file
        // has a result, ignore them; and 100% only means the bytes were sent, not that the server accepted them.
        if (file.HasError || file.IsCompleted) return;
        file.Progress = progress;
        file.StatusMessage = progress >= 100 ? L["Processing"].Value! : L["Uploading"].Value!;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Called by JS when a single file upload completes (success or error).
    /// </summary>
    [JSInvokable]
    public async Task OnUploadComplete(int rowId, JsUploadResult result)
    {
        if (_disposed) return;
        var uploadingFile = _rows.Find(rowId);
        if (uploadingFile == null) return;
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
            catch (JsonException exception)
            {
                Logger.LogWarning(exception, "File upload response JSON could not be parsed.");
                uploadingFile.HasError = true;
                uploadingFile.ErrorMessage = L["UploadFailed"];
                uploadingFile.StatusMessage = null;
            }
        }
        else
        {
            // sufi-chain#479: one failure surface (the row's alert) with the server's reason when it gave one.
            uploadingFile.HasError = true;
            uploadingFile.IsCompleted = false;
            uploadingFile.Progress = 0;
            Logger.LogWarning("File upload failed: {Error}", result.Error);
            var reason = GetUploadRejectionReason(result.Error);
            uploadingFile.ErrorMessage = reason == null ? L["UploadFailed"] : L["UploadRejected", reason];
            uploadingFile.StatusMessage = null;
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

    /// <summary>
    /// The server's rejection reason (e.g. "File type 'x' is not allowed"), or null when the error is only a
    /// bare status or empty, in which case the generic localized message is shown.
    /// </summary>
    internal static string? GetUploadRejectionReason(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return null;
        var trimmed = error.Trim();
        if (trimmed.StartsWith("Upload failed with status", StringComparison.OrdinalIgnoreCase))
            return null;
        return trimmed.Length > 300 ? trimmed[..300] : trimmed;
    }

    private void CancelUpload(UploadingFile file)
    {
        _rows.Remove(file);
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
        // sufi-chain#479: localized unit and Persian digits in fa («۱۰۰ مگابایت»).
        return FileSizeText.Format(bytes, (unitKey, number) => L[unitKey, number].Value!);
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
                        _rows.Remove(uploadingFile);
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
