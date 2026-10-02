using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SufiChain.SufiPlatform.FileManager.Blazor.Components.FileManager;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.UI.Timing;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Pages;

public partial class ImageEditor : FileManagerComponentBase
{
    public const string AssetsRoute = "/panel/admin/file-manager/assets";

    public static string GetRoute(Guid assetId) => $"{AssetsRoute}/{assetId}/edit";

    [Parameter] public Guid AssetId { get; set; }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected ImageEditorInterop ImageEditorInterop { get; set; } = default!;
    [Inject] protected IFileItemAppService FileItemAppService { get; set; } = default!;
    [Inject] protected IFileItemUrlProvider FileItemUrlProvider { get; set; } = default!;

    private string? _appliedTitle;
    [Inject] protected IClock Clock { get; set; } = default!;

    private FileItemDto? _file;
    private Guid? _loadedAssetId;
    private ElementReference _imageElement;
    private string? _editorId;
    private string _imageSrc = "";
    private bool _assetLoading = true;
    private bool _notFound;
    private bool _isLoading = true;
    private bool _isSaving;
    private bool _imageReady;
    private bool _editorStarting;
    private bool _editorInitFailed;
    private bool _canUndo;
    private bool _canRedo;
    private string _aspectRatioKey = "free";
    private int _brightness;
    private int _contrast;
    private int _saturation;
    private bool _grayscale;
    private bool _sepia;
    private bool _cropBoxVisible;
    private string _saveFormat = "image/png";
    private double _saveQuality = 0.92;
    private bool _mobileEditorMenuOpen;
    private string _saveAsFileName = "";

    protected override void OnInitialized()
    {
        ApplyTitle();
    }

    protected override async Task OnParametersSetAsync()
    {
        ApplyTitle();

        if (_loadedAssetId == AssetId)
        {
            return;
        }

        _loadedAssetId = AssetId;
        await LoadAssetAsync();
    }

    private void ApplyTitle()
    {
        var title = L["ImageEditor"].Value;
        if (string.Equals(_appliedTitle, title, StringComparison.Ordinal))
        {
            return;
        }

        _appliedTitle = title;
        PageLayout.Title = title;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (_imageReady)
        {
            await TryInitEditorAsync();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _ = DestroyEditorQuietlyAsync();
        }

        base.Dispose(disposing);
    }

    private async Task LoadAssetAsync()
    {
        _assetLoading = true;
        _notFound = false;
        _file = null;
        _imageReady = false;
        _editorInitFailed = false;
        _isLoading = true;
        ResetControls();
        await DestroyEditorQuietlyAsync();

        try
        {
            await ExecuteWithLoadingAsync(LoadAssetCoreAsync, "load-asset", LoadingBehavior.PageProgress);
        }
        finally
        {
            _assetLoading = false;
        }
    }

    private async Task LoadAssetCoreAsync()
    {
        try
        {
            var file = await FileItemAppService.GetAsync(AssetId);
            if (IsDisposed)
            {
                return;
            }

            if (file.FileType != FileType.Image)
            {
                _notFound = true;
                return;
            }

            _file = file;
            _imageSrc = FileItemUrlProvider.GetStreamUrl(
                file.Id,
                Clock.Now.Ticks,
                file.StructureBaseUrl,
                file.StructureIsPublicAccess,
                file.BlobName,
                file.TenantId,
                file.StructureStorageProvider);
            PrepareSaveAsFileName();
        }
        catch (Exception ex)
        {
            _notFound = true;
            _file = null;
            await NotifyOperationFailedAsync(ex, "FailedToLoadImageEditor");
        }
    }

    private void ResetControls()
    {
        _canUndo = false;
        _canRedo = false;
        _aspectRatioKey = "free";
        _brightness = 0;
        _contrast = 0;
        _saturation = 0;
        _grayscale = false;
        _sepia = false;
        _cropBoxVisible = false;
        _saveFormat = "image/png";
        _saveQuality = 0.92;
        _mobileEditorMenuOpen = false;
        _saveAsFileName = "";
        _imageSrc = "";
    }

    private async Task OnImageLoaded()
    {
        if (_file == null || string.IsNullOrEmpty(_imageSrc))
        {
            return;
        }

        _imageReady = true;
        _isLoading = false;
        await TryInitEditorAsync();
    }

    private async Task OnImageError()
    {
        _isLoading = false;
        _imageReady = false;
        await Notify.ErrorAsync(L["FailedToLoadImage"]);
    }

    private async Task TryInitEditorAsync()
    {
        if (!IsInteractive || !_imageReady || _editorInitFailed || _file == null || !string.IsNullOrEmpty(_editorId) || _editorStarting)
        {
            return;
        }

        _editorStarting = true;
        try
        {
            _editorId = await ImageEditorInterop.InitAsync(_imageElement, options: null);
            await UpdateCanUndoRedoAsync();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            _editorInitFailed = true;
            await NotifyOperationFailedAsync(ex, "FailedToLoadImageEditor");
        }
        finally
        {
            _editorStarting = false;
        }
    }

    private async Task UpdateCanUndoRedoAsync()
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        _canUndo = await ImageEditorInterop.CanUndoAsync(_editorId);
        _canRedo = await ImageEditorInterop.CanRedoAsync(_editorId);
    }

    private async Task RotateAsync(double deg)
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        await ImageEditorInterop.RotateAsync(_editorId, deg);
        await UpdateCanUndoRedoAsync();
    }

    private async Task FlipXAsync()
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        await ImageEditorInterop.FlipXAsync(_editorId);
        await UpdateCanUndoRedoAsync();
    }

    private async Task FlipYAsync()
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        await ImageEditorInterop.FlipYAsync(_editorId);
        await UpdateCanUndoRedoAsync();
    }

    private async Task ResetAsync()
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        await ImageEditorInterop.ResetAsync(_editorId);
        _brightness = _contrast = _saturation = 0;
        _grayscale = _sepia = false;
        await UpdateCanUndoRedoAsync();
    }

    private async Task UndoAsync()
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        await ImageEditorInterop.UndoAsync(_editorId);
        await UpdateCanUndoRedoAsync();
    }

    private async Task RedoAsync()
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        await ImageEditorInterop.RedoAsync(_editorId);
        await UpdateCanUndoRedoAsync();
    }

    private async Task ZoomAsync(double ratio)
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        await ImageEditorInterop.ZoomAsync(_editorId, ratio);
    }

    private async Task ToggleCropBoxAsync()
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        _cropBoxVisible = await ImageEditorInterop.ToggleCropBoxAsync(_editorId);
    }

    private async Task OnAspectRatioChanged(string? value)
    {
        _aspectRatioKey = value ?? "free";
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        var ratio = _aspectRatioKey switch
        {
            "1" => 1.0,
            "4:3" => 4.0 / 3,
            "16:9" => 16.0 / 9,
            _ => double.NaN
        };
        await ImageEditorInterop.SetAspectRatioAsync(_editorId, ratio);
        await UpdateCanUndoRedoAsync();
    }

    private async Task OnFilterChanged()
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        await ImageEditorInterop.ApplyFilterAsync(_editorId, "brightness", _brightness);
        await ImageEditorInterop.ApplyFilterAsync(_editorId, "contrast", _contrast);
        await ImageEditorInterop.ApplyFilterAsync(_editorId, "saturation", _saturation);
        await UpdateCanUndoRedoAsync();
    }

    private async Task OnBrightnessChanged(int? value)
    {
        _brightness = value ?? 0;
        await OnFilterChanged();
    }

    private async Task OnContrastChanged(int? value)
    {
        _contrast = value ?? 0;
        await OnFilterChanged();
    }

    private async Task OnSaturationChanged(int? value)
    {
        _saturation = value ?? 0;
        await OnFilterChanged();
    }

    private void OnSaveQualityChanged(double? value)
    {
        _saveQuality = value ?? 0.92;
    }

    private void OnSaveFormatChanged(string? value)
    {
        _saveFormat = string.IsNullOrWhiteSpace(value) ? "image/png" : value;
        SyncSaveAsExtension();
    }

    private async Task OnGrayscaleChanged(bool value)
    {
        _grayscale = value;
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        await ImageEditorInterop.ApplyFilterAsync(_editorId, "grayscale", value);
        await UpdateCanUndoRedoAsync();
    }

    private async Task OnSepiaChanged(bool value)
    {
        _sepia = value;
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        await ImageEditorInterop.ApplyFilterAsync(_editorId, "sepia", value);
        await UpdateCanUndoRedoAsync();
    }

    private async Task SaveAsync()
    {
        if (_file == null || string.IsNullOrEmpty(_editorId) || _isSaving)
        {
            return;
        }

        await ExecuteWithLoadingAsync(SaveCoreAsync, "save-image", LoadingBehavior.None);
    }

    private async Task SaveCoreAsync()
    {
        if (_file == null)
        {
            return;
        }

        _isSaving = true;
        try
        {
            var content = await ExportEditedContentAsync();
            if (content == null)
            {
                await Notify.ErrorAsync(L["SaveFailed"]);
                return;
            }

            var ext = ExtensionForFormat(_saveFormat);
            var fileName = Path.ChangeExtension(_file.OriginalName, ext);
            await FileItemAppService.ReplaceContentAsync(_file.Id, new ReplaceFileContentInput
            {
                Content = content,
                FileName = fileName,
                MimeType = _saveFormat
            });

            await Notify.SuccessAsync(L["SavedSuccessfully"]);
            await DestroyEditorQuietlyAsync();
            NavigationManager.NavigateTo(AssetsRoute);
        }
        catch (Exception ex)
        {
            await NotifyOperationFailedAsync(ex, "SaveFailed");
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task SaveAsAsync()
    {
        if (_file == null || string.IsNullOrEmpty(_editorId) || _isSaving)
        {
            return;
        }

        var newName = _saveAsFileName?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        await ExecuteWithLoadingAsync(() => SaveAsCoreAsync(newName), "save-as-image", LoadingBehavior.None);
    }

    private async Task SaveAsCoreAsync(string newName)
    {
        if (_file == null)
        {
            return;
        }

        var ext = ExtensionForFormat(_saveFormat);
        if (!newName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
        {
            newName += ext;
        }

        _isSaving = true;
        try
        {
            var content = await ExportEditedContentAsync();
            if (content == null)
            {
                await Notify.ErrorAsync(L["SaveFailed"]);
                return;
            }

            await FileItemAppService.SaveAsAsync(_file.Id, new SaveAsFileInput
            {
                SourceId = _file.Id,
                FileName = newName,
                Content = content,
                MimeType = _saveFormat,
                FolderId = _file.FolderId
            });

            await Notify.SuccessAsync(L["SavedSuccessfully"]);
            await DestroyEditorQuietlyAsync();
            NavigationManager.NavigateTo(AssetsRoute);
        }
        catch (Exception ex)
        {
            await NotifyOperationFailedAsync(ex, "SaveFailed");
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task<byte[]?> ExportEditedContentAsync()
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return null;
        }

        await ImageEditorInterop.ApplyFilterAsync(_editorId, "brightness", _brightness);
        await ImageEditorInterop.ApplyFilterAsync(_editorId, "contrast", _contrast);
        await ImageEditorInterop.ApplyFilterAsync(_editorId, "saturation", _saturation);
        await ImageEditorInterop.ApplyFilterAsync(_editorId, "grayscale", _grayscale);
        await ImageEditorInterop.ApplyFilterAsync(_editorId, "sepia", _sepia);

        var dataUrl = await ImageEditorInterop.GetCroppedDataUrlAsync(_editorId, _saveFormat, _saveQuality);
        if (string.IsNullOrEmpty(dataUrl))
        {
            return null;
        }

        var base64 = dataUrl.Contains(',') ? dataUrl.Split(',', 2)[1] : dataUrl;
        return Convert.FromBase64String(base64);
    }

    private async Task CancelAsync()
    {
        await DestroyEditorQuietlyAsync();
        NavigationManager.NavigateTo(AssetsRoute);
    }

    private void PrepareSaveAsFileName()
    {
        var ext = ExtensionForFormat(_saveFormat);
        var stem = Path.GetFileNameWithoutExtension(_file?.OriginalName ?? "");
        _saveAsFileName = Path.ChangeExtension(stem + L["CopySuffix"].Value, ext);
    }

    private void SyncSaveAsExtension()
    {
        if (string.IsNullOrWhiteSpace(_saveAsFileName))
        {
            PrepareSaveAsFileName();
            return;
        }

        var current = Path.GetExtension(_saveAsFileName);
        if (!IsKnownImageExtension(current))
        {
            return;
        }

        _saveAsFileName = Path.ChangeExtension(_saveAsFileName, ExtensionForFormat(_saveFormat));
    }

    private static string ExtensionForFormat(string format) => format switch
    {
        "image/jpeg" => ".jpg",
        "image/webp" => ".webp",
        _ => ".png"
    };

    private static bool IsKnownImageExtension(string extension) =>
        extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
        || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
        || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
        || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);

    private async Task DestroyEditorQuietlyAsync()
    {
        if (string.IsNullOrEmpty(_editorId))
        {
            return;
        }

        var editorId = _editorId;
        _editorId = null;
        try
        {
            await ImageEditorInterop.DestroyAsync(editorId);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or ObjectDisposedException or TaskCanceledException)
        {
        }
    }
}
