using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiBlazor.Components.Feedback;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Components.Browser;

/// <summary>
/// File browser component with grid/list views, filtering, and bulk operations.
/// </summary>
public partial class FileBrowser : FileManagerComponentBase, IDisposable
{
    private record SortOption(string Label, string Value);

    private record StructureFilterOption(string? Key, string DisplayName);

    private static readonly FileType?[] _fileTypeOptions =
        new FileType?[] { null, FileType.Image, FileType.Video, FileType.Document, FileType.Audio };

    private List<SortOption> _sortOptions = new();
    private List<StructureFilterOption> _structureFilterOptions = new();

    private string GetFileTypeLabel(FileType? type)
    {
        if (type == null) return L["AllTypes"].Value!;
        return type switch
        {
            FileType.Image => L["FileTypeImage"].Value!,
            FileType.Video => L["FileTypeVideo"].Value!,
            FileType.Document => L["FileTypeDocument"].Value!,
            FileType.Audio => L["FileTypeAudio"].Value!,
            _ => type.ToString()!
        };
    }

    protected override void OnInitialized()
    {
        _sortOptions = new List<SortOption>
        {
            new(L["SortNewestFirst"].Value!, "CreationTime DESC"),
            new(L["SortOldestFirst"].Value!, "CreationTime ASC"),
            new(L["SortNameAZ"].Value!, "OriginalName ASC"),
            new(L["SortNameZA"].Value!, "OriginalName DESC"),
            new(L["SortLargestFirst"].Value!, "Size DESC"),
            new(L["SortSmallestFirst"].Value!, "Size ASC")
        };
    }

    [Inject]
    protected IFileItemAppService FileItemAppService { get; set; } = default!;
    [Inject]
    protected IFileStructureAppService FileStructureAppService { get; set; } = default!;
    [Inject]
    protected IFileItemUrlProvider FileItemUrlProvider { get; set; } = default!;
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public string? StructureKey { get; set; }
    [Parameter] public string? EntityType { get; set; }
    [Parameter] public Guid? EntityId { get; set; }
    [Parameter] public EventCallback<FileItemDto> OnFileSelected { get; set; }


    private List<FileItemDto> _fileItems = new();
    private HashSet<Guid> _selectedItems = new();
    private bool _isLoading = false;
    private string _searchKeyword = "";
    private string? _filterStructureKey = null;
    private FileType? _filterFileType = null;
    private bool _showOnlyPublic = false;
    private string _sortBy = "CreationTime DESC";
    private ViewMode _viewMode = ViewMode.Grid;
    private bool _selectAll = false;
    private StorageQuotaDto? _storageQuota;
    private bool _uploadModalOpen = false;
    private bool _deleteModalOpen = false;
    private readonly CancellationTokenSource _cts = new();


    private int _currentPage = 1;
    private int _pageSize = 24;
    private int _totalCount = 0;
    private int _totalPages => (_totalCount + _pageSize - 1) / _pageSize;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await LoadStructureFilterOptions();
            await LoadFiles();
            await LoadStorageQuota();
        }
    }

    private async Task LoadFiles()
    {
        _isLoading = true;
        try
        {
            var input = new GetFileListInput
            {
                Keyword = _searchKeyword,
                FileType = _filterFileType,
                EntityType = EntityType,
                EntityId = EntityId,
                StructureKey = _filterStructureKey,
                OnlyFromPublicStructures = _showOnlyPublic ? true : null,
                Sorting = _sortBy,
                SkipCount = (_currentPage - 1) * _pageSize,
                MaxResultCount = _pageSize
            };

            var result = await FileItemAppService.GetListAsync(input);
            _fileItems = result.Items.ToList();
            _totalCount = (int)result.TotalCount;
        }
        catch (Exception ex)
        {
            await Notify.ErrorAsync(L["FailedToLoadFiles", ex.Message]);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Refreshes the file list and storage quota. Can be called by the parent (e.g. on page load).
    /// </summary>
    public async Task RefreshAsync()
    {
        await RefreshFiles();
    }

    private async Task LoadStorageQuota()
    {
        try
        {
            _storageQuota = await FileItemAppService.GetStorageQuotaAsync();
        }
        catch
        {
            // Ignore quota errors
        }
    }

    private async Task RefreshFiles()
    {
        _selectedItems.Clear();
        _selectAll = false;
        await LoadFiles();
        await LoadStorageQuota();
    }

    private async Task OnSearchKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            _currentPage = 1;
            await LoadFiles();
        }
    }

    private async Task LoadStructureFilterOptions()
    {
        try
        {
            var result = await FileStructureAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                Sorting = "DisplayName"
            });
            _structureFilterOptions = result.Items
                .Select(s => new StructureFilterOption(s.Key, ResolveStructureText(s)))
                .Prepend(new StructureFilterOption(null, L["AllStructures"].Value!))
                .ToList();
        }
        catch
        {
            _structureFilterOptions = new List<StructureFilterOption> { new(null, L["AllStructures"].Value!) };
        }
    }

    private async Task OnStructureFilterChanged()
    {
        _currentPage = 1;
        await LoadFiles();
    }

    private async Task OnFileTypeFilterChanged()
    {
        _currentPage = 1;
        await LoadFiles();
    }

    private async Task OnPublicFilterChanged()
    {
        _currentPage = 1;
        await LoadFiles();
    }

    private async Task OnSortByChanged()
    {
        await LoadFiles();
    }

    private async Task ChangePage(int page)
    {
        _currentPage = page;
        await LoadFiles();
    }

    private void ToggleSelection(Guid id, bool selected)
    {
        if (selected)
            _selectedItems.Add(id);
        else
            _selectedItems.Remove(id);

        _selectAll = _selectedItems.Count == _fileItems.Count;
        StateHasChanged();
    }

    private void ToggleSelectAll(bool selected)
    {
        _selectAll = selected;
        if (selected)
        {
            foreach (var item in _fileItems)
                _selectedItems.Add(item.Id);
        }
        else
        {
            _selectedItems.Clear();
        }
        StateHasChanged();
    }

    private void DeleteSelected()
    {
        if (!_selectedItems.Any()) return;
        _deleteModalOpen = true;
    }

    private void CancelDelete()
    {
        _deleteModalOpen = false;
    }

    private async Task ConfirmDeleteSelected()
    {
        if (!_selectedItems.Any()) return;

        try
        {
            await FileItemAppService.DeleteManyAsync(_selectedItems.ToArray());
            await Notify.SuccessAsync(L["FilesDeletedSuccessfully"]);
            _selectedItems.Clear();
            await RefreshFiles();
        }
        catch (Exception ex)
        {
            await Notify.ErrorAsync(L["FailedToDelete", ex.Message]);
        }
        finally
        {
            _deleteModalOpen = false;
        }
    }

    private void DeleteFile(Guid id)
    {
        _selectedItems.Clear();
        _selectedItems.Add(id);
        _deleteModalOpen = true;
    }

    private void ShowUploadModal()
    {
        _uploadModalOpen = true;
    }

    private async Task OnUploadCompleted(List<FileItemDto> uploadedFiles)
    {
        _uploadModalOpen = false;
        
        if (uploadedFiles.Count > 0)
        {
            await Notify.SuccessAsync(L["SuccessfullyUploadedFiles", uploadedFiles.Count]);
        }
        
        await RefreshFiles();
    }

    private async Task OpenPreview(FileItemDto item)
    {
        await OnFileSelected.InvokeAsync(item);
    }

    private string GetThumbnailUrl(FileItemDto file)
    {
        return FileItemUrlProvider.GetThumbnailUrl(file.Id, (file.LastModificationTime ?? file.CreationTime).Ticks, file.StructureBaseUrl, file.StructureIsPublicAccess, file.ThumbnailBlobName, file.TenantId, file.StructureStorageProvider);
    }

    private string GetDownloadUrl(FileItemDto file)
    {
        return FileItemUrlProvider.GetDownloadUrl(file.Id, file.StructureBaseUrl, file.StructureIsPublicAccess, file.BlobName, file.TenantId, file.StructureStorageProvider);
    }

    private string GetFileIconName(FileType fileType)
    {
        return fileType switch
        {
            FileType.Image => "file",
            FileType.Video => "file",
            FileType.Document => "file",
            FileType.Audio => "file",
            _ => "file"
        };
    }

    private SbColor GetFileTypeColor(FileType fileType)
    {
        return fileType switch
        {
            FileType.Image => SbColor.Success,
            FileType.Video => SbColor.Info,
            FileType.Document => SbColor.Warning,
            FileType.Audio => SbColor.Primary,
            _ => SbColor.Default
        };
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

    private string GetItemClass(Guid itemId)
    {
        return _selectedItems.Contains(itemId) ? "file-browser-item selected" : "file-browser-item";
    }


    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    private enum ViewMode
    {
        Grid,
        List
    }
}
