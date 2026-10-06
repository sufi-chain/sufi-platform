using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components.Feedback;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Pages;

public partial class AssetManager : FileManagerComponentBase, IDisposable
{
    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected IFileItemUrlProvider FileItemUrlProvider { get; set; } = default!;
    [Inject] protected IFileItemAppService FileItemAppService { get; set; } = default!;

    private bool _uploadModalOpen;
    private bool _viewerModalOpen;
    private FileItemDto? _viewingItem;
    private List<FileItemDto> _selectedItems = new();
    private bool _showProperties = false;
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed = false;

    private bool _quickShareOpen;
    private Guid? _quickShareFileId;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        SetupPageLayout();
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["Menu:AssetManager"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    private void OpenUploadModal()
    {
        _uploadModalOpen = true;
    }

    private void OnUploadModalOpenChanged(bool value)
    {
        _uploadModalOpen = value;
    }

    private void OnViewerModalOpenChanged(bool value)
    {
        _viewerModalOpen = value;
    }

    private async Task OnUploadCompleted(List<FileItemDto> items)
    {
        _uploadModalOpen = false;
        await Notify.SuccessAsync(L["SuccessfullyUploadedFiles", items.Count]);
    }

    private void HandleFileOpen(FileItemDto file)
    {
        _viewingItem = file;
        _viewerModalOpen = true;
    }

    private void HandleSelectionChanged(List<FileItemDto> items)
    {
        _selectedItems = items;
    }

    private void HandleEditImage(FileItemDto file)
    {
        NavigationManager.NavigateTo(ImageEditor.GetRoute(file.Id));
    }

    private void OpenQuickShare()
    {
        if (_viewingItem != null)
        {
            _quickShareFileId = _viewingItem.Id;
            _quickShareOpen = true;
        }
    }

    private void OpenImageEditor()
    {
        if (_viewingItem != null)
        {
            NavigationManager.NavigateTo(ImageEditor.GetRoute(_viewingItem.Id));
        }
    }

    private void CloseViewerModal()
    {
        _viewerModalOpen = false;
    }

    private void HandleShowPropertiesPanelChanged(bool show)
    {
        _showProperties = show;
    }

    private string GetDownloadUrl(FileItemDto file)
    {
        return FileItemUrlProvider.GetDownloadUrl(file.Id, file.StructureBaseUrl, file.StructureIsPublicAccess, file.BlobName, file.TenantId, file.StructureStorageProvider);
    }

    /// <summary>
    /// Key that changes when file content changes, so FileImage/FileVideo reload with fresh cache-busted URLs.
    /// </summary>
    private static string GetViewerMediaKey(FileItemDto item)
    {
        var ticks = (item.LastModificationTime ?? item.CreationTime).Ticks;
        return $"{item.Id}_{ticks}";
    }


    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DisposeCancellationSource(_cts);
    }
}
