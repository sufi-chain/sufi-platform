using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SufiChain.SufiBlazor.Components.Feedback;
using SufiChain.SufiAbp.FileManager.Blazor.Public.Services;
using SufiChain.SufiAbp.FileManager.Blazor.Services;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.FileManager.Blazor.Components.Upload;

/// <summary>
/// Quick image uploader component with preview and simple upload workflow.
/// Uses direct HTTP upload (bypasses SignalR) to avoid circuit timeout and frozen UI.
/// </summary>
public partial class QuickImageUploader : FileManagerComponentBase, IDisposable
{
    [Inject]
    protected IFileItemAppService FileItemAppService { get; set; } = default!;

    [Inject]
    protected IFileStructureAppService FileStructureAppService { get; set; } = default!;

    [Inject]
    protected IFileItemUrlProvider FileItemUrlProvider { get; set; } = default!;

    [Inject]
    protected FileManagerJsInterop JsInterop { get; set; } = default!;

    [Inject]
    protected IFileUploadAccessTokenProvider? AccessTokenProvider { get; set; }

    [Parameter] public string? StructureKey { get; set; }
    [Parameter] public bool ShowStructureSelector { get; set; } = false;
    /// <summary>
    /// When true and FolderPath is not passed, shows an input for the user to type the target folder path. Default false = upload to root.
    /// </summary>
    [Parameter] public bool ShowFolderPathField { get; set; } = false;
    /// <summary>
    /// Target folder path (e.g. "/web/tourist"). When set, folders are created if missing.
    /// When not set, an optional input is shown. Empty = root.
    /// </summary>
    [Parameter] public string? FolderPath { get; set; }
    [Parameter] public string? EntityType { get; set; }
    [Parameter] public Guid? EntityId { get; set; }
    [Parameter] public bool AutoConfirm { get; set; } = true;
    [Parameter] public string Placeholder { get; set; } = "Max 5MB, JPG/PNG/WebP";
    [Parameter] public long MaxFileSize { get; set; } = 5 * 1024 * 1024; // 5MB
    [Parameter] public EventCallback<FileItemDto> OnImageUploaded { get; set; }
    [Parameter] public string CssClass { get; set; } = "";

    private FileItemDto? _uploadedImage;
    private bool _isUploading = false;
    private int _uploadProgress = 0;
    private string? _errorMessage;
    private bool _disposed;
    private DotNetObjectReference<QuickImageUploader>? _dotNetRef;
    private bool _changeHandlerRegistered;
    private readonly string _fileInputId = $"quickImageUploader-fileInput-{Guid.NewGuid():N}";
    private List<FileStructureDto> _availableStructures = new();
    private string? _selectedStructureKey;
    private string _userFolderPath = "";
    private bool _configModalOpen;

    private bool _hasConfigOptions =>
        (ShowStructureSelector && _availableStructures.Any()) ||
        (ShowFolderPathField && string.IsNullOrEmpty(FolderPath));

    protected override void OnInitialized()
    {
        _selectedStructureKey = StructureKey ?? SufiChain.SufiAbp.FileManager.Configuration.FileStructureKeys.General;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && !_changeHandlerRegistered)
        {
            _changeHandlerRegistered = true;
            _dotNetRef = DotNetObjectReference.Create(this);
            await JsInterop.RegisterFileInputChangeAsync(_fileInputId, _dotNetRef);

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
            StateHasChanged();
        }
    }

    private async Task OnStructureChanged(string? newStructureKey)
    {
        _selectedStructureKey = newStructureKey;
        StateHasChanged();
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
            StructureKey = _selectedStructureKey ?? StructureKey ?? SufiChain.SufiAbp.FileManager.Configuration.FileStructureKeys.General,
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
    /// Called by JS with selected file infos before upload starts.
    /// Throws to abort upload when validation fails (JS catches and stops).
    /// </summary>
    [JSInvokable]
    public void OnFilesSelected(JsFileInfo[] fileInfos)
    {
        if (_disposed || fileInfos == null) return;

        _errorMessage = null;
        if (fileInfos.Length == 0) throw new InvalidOperationException("No files selected");
        if (fileInfos.Length > 1)
        {
            _errorMessage = L["OnlySingleImageAllowed"].Value;
            InvokeAsync(StateHasChanged);
            throw new InvalidOperationException(_errorMessage);
        }
        var info = fileInfos[0];
        if (info.Size > MaxFileSize)
        {
            _errorMessage = L["FileSizeExceedsMaximum", (MaxFileSize / (1024 * 1024)).ToString()].Value;
            InvokeAsync(StateHasChanged);
            throw new InvalidOperationException(_errorMessage);
        }

        _isUploading = true;
        _errorMessage = null;
        _uploadProgress = 0;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Called by JS with progress for the file (0-100).
    /// </summary>
    [JSInvokable]
    public void OnUploadProgress(int fileIndex, int progress)
    {
        if (_disposed) return;
        _uploadProgress = progress;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Called by JS when the upload completes (success or error).
    /// </summary>
    [JSInvokable]
    public async Task OnUploadComplete(int fileIndex, JsUploadResult result)
    {
        if (_disposed) return;

        try
        {
            if (result.Success && result.Data.HasValue)
            {
                var dto = result.Data.Value.Deserialize<FileItemDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (dto != null)
                {
                    _uploadedImage = dto;
                    _uploadProgress = 100;
                    await OnImageUploaded.InvokeAsync(dto);
                    await Notify.SuccessAsync(L["ImageUploadedSuccessfully"]);
                }
            }
            else
            {
                _errorMessage = result.Error ?? L["UploadFailed", ""].Value;
                await Notify.ErrorAsync(L["UploadFailed", _errorMessage]);
            }
        }
        finally
        {
            if (!_disposed)
            {
                _isUploading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }

    /// <summary>
    /// Clears the displayed image so the uploader is ready for another upload.
    /// Use when the uploaded file was handed off (e.g. to a gallery) and the uploader should reset.
    /// </summary>
    public void Clear()
    {
        _uploadedImage = null;
        _errorMessage = null;
        _isUploading = false;
        _uploadProgress = 0;
        StateHasChanged();
    }

    private async Task RemoveImage()
    {
        if (_uploadedImage != null)
        {
            try
            {
                await FileItemAppService.DeleteAsync(_uploadedImage.Id);
                _uploadedImage = null;
                await Notify.SuccessAsync(L["ImageRemoved"]);
            }
            catch (Exception ex)
            {
                await Notify.ErrorAsync(L["FailedToRemoveImage", ex.Message]);
            }
        }
    }

    private string GetPreviewUrl()
    {
        if (_uploadedImage == null)
            return "#";

        if (!string.IsNullOrEmpty(_uploadedImage.ThumbnailBlobName))
        {
            return FileItemUrlProvider.GetThumbnailUrl(_uploadedImage.Id, (_uploadedImage.LastModificationTime ?? _uploadedImage.CreationTime).Ticks, _uploadedImage.StructureBaseUrl, _uploadedImage.StructureIsPublicAccess, _uploadedImage.ThumbnailBlobName, _uploadedImage.TenantId, _uploadedImage.StructureStorageProvider);
        }

        return FileItemUrlProvider.GetDownloadUrl(_uploadedImage.Id, _uploadedImage.StructureBaseUrl, _uploadedImage.StructureIsPublicAccess, _uploadedImage.BlobName, _uploadedImage.TenantId, _uploadedImage.StructureStorageProvider);
    }
}
