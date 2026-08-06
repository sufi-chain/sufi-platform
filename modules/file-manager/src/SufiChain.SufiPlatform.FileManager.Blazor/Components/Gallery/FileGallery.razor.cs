using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiBlazor.Components.Feedback;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileTypes;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Components.Gallery;

/// <summary>
/// Gallery component for displaying and managing file items with grid/list views.
/// </summary>
public partial class FileGallery : FileManagerComponentBase
{
    private static readonly FileType?[] _fileTypeOptions =
        new FileType?[] { null, FileType.Image, FileType.Video, FileType.Document };

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

    [Inject]
    protected IFileItemAppService FileItemAppService { get; set; } = default!;
    [Inject]
    protected IFileItemUrlProvider FileItemUrlProvider { get; set; } = default!;

    [Parameter] public string? StructureKey { get; set; }
    [Parameter] public string? EntityType { get; set; }
    [Parameter] public Guid? EntityId { get; set; }
    [Parameter] public bool Selectable { get; set; } = false;
    [Parameter] public bool ShowActions { get; set; } = true;
    [Parameter] public int PageSize { get; set; } = 12;
    [Parameter] public EventCallback<FileItemDto> OnFileSelected { get; set; }
    [Parameter] public EventCallback<List<Guid>> OnSelectionChanged { get; set; }
    [Parameter] public string CssClass { get; set; } = "";

    private enum ViewMode { Grid, List }
    

    private ViewMode _viewMode = ViewMode.Grid;
    private List<FileItemDto> _fileItems = new();
    private HashSet<Guid> _selectedItems = new();
    private FileItemDto? _selectedFileItem;
    private bool _lightboxOpen = false;
    private bool _isLoading = false;
    private string? _searchKeyword;
    private FileType? _selectedFileType;
    private int _currentPage = 1;
    private int _totalCount = 0;
    private int _totalPages = 0;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await LoadFileItems();
        }
    }

    private async Task LoadFileItems()
    {
        _isLoading = true;

        try
        {
            var input = new GetFileListInput
            {
                Keyword = _searchKeyword,
                FileType = _selectedFileType,
                EntityType = EntityType,
                EntityId = EntityId,
                StructureKey = StructureKey,
                SkipCount = (_currentPage - 1) * PageSize,
                MaxResultCount = PageSize,
                Sorting = "CreationTime DESC"
            };

            var result = await FileItemAppService.GetListAsync(input);
            _fileItems = result.Items.ToList();
            _totalCount = (int)result.TotalCount;
            _totalPages = (int)Math.Ceiling((double)_totalCount / PageSize);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load files");
            await Notify.ErrorAsync(L["FailedToLoadFiles", ex.Message]);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadPage(int page)
    {
        _currentPage = page;
        await LoadFileItems();
    }

    private void SetViewMode(ViewMode mode)
    {
        _viewMode = mode;
    }

    private async Task ClearFilters()
    {
        _searchKeyword = null;
        _selectedFileType = null;
        _currentPage = 1;
        await LoadFileItems();
    }

    private async Task SelectFileItem(FileItemDto item)
    {
        if (Selectable)
        {
            if (_selectedItems.Contains(item.Id))
                _selectedItems.Remove(item.Id);
            else
                _selectedItems.Add(item.Id);

            await OnSelectionChanged.InvokeAsync(_selectedItems.ToList());
        }

        await OnFileSelected.InvokeAsync(item);
    }

    private void ViewFileItem(FileItemDto item)
    {
        _selectedFileItem = item;
        _lightboxOpen = true;
    }

    private async Task EditFileItem(FileItemDto item)
    {
        await Notify.InfoAsync(L["EditFunctionalityComingSoon"]);
    }

    private async Task DeleteFileItem(FileItemDto item)
    {
        try
        {
            await FileItemAppService.DeleteAsync(item.Id);
            await Notify.SuccessAsync(L["FileDeletedSuccessfully"]);
            await LoadFileItems();
        }
        catch (Exception ex)
        {
            await Notify.ErrorAsync(L["FailedToDelete", ex.Message]);
        }
    }

    private string GetFileUrl(FileItemDto item)
    {
        return FileItemUrlProvider.GetDownloadUrl(item.Id, item.StructureBaseUrl, item.StructureIsPublicAccess, item.BlobName, item.TenantId, item.StructureStorageProvider);
    }

    private string GetDownloadUrl(FileItemDto item)
    {
        return FileItemUrlProvider.GetDownloadUrl(item.Id, item.StructureBaseUrl, item.StructureIsPublicAccess, item.BlobName, item.TenantId, item.StructureStorageProvider);
    }

    private SbColor GetFileTypeColor(FileType type)
    {
        return type switch
        {
            FileType.Image => SbColor.Primary,
            FileType.Video => SbColor.Success,
            FileType.Document => SbColor.Info,
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

 
}
