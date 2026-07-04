using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiAbp.FileManager.Blazor.Public.Services;
using SufiChain.SufiAbp.FileManager.Permissions;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using SufiChain.SufiAbp.FileManager.FileTypes;
using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.Application.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace SufiChain.SufiAbp.FileManager.Blazor.Pages;

public partial class FileStats : FileManagerComponentBase
{
    [Inject] private IFileItemAppService FileItemAppService { get; set; } = default!;
    [Inject] private IFileStructureAppService FileStructureAppService { get; set; } = default!;
    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected IFileItemUrlProvider FileItemUrlProvider { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    private List<FileItemDto> _recentFiles = new();
    private bool _viewerModalOpen;
    private FileItemDto? _viewingItem;
   private List<FileStructureDto> _structures = new();
   private FileStatistics _statistics = new();
    private bool _canDelete;

    protected override void OnInitialized()
    {
        SetupPageLayout();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await LoadData();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["Menu:FileStats"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

   private async Task LoadData()
   {
        _canDelete = await AuthorizationService.IsGrantedAsync(FileManagerPermissions.FileItems.Delete);
       await Task.WhenAll(
           LoadRecentFiles(),
           LoadStructures(),
           RefreshStats()
       );

        StateHasChanged(); // Update UI with results
    }

    private async Task LoadRecentFiles()
    {
        try
        {
            var result = await FileItemAppService.GetListAsync(new GetFileListInput
            {
                Sorting = "CreationTime DESC",
                MaxResultCount = 10
            });
            _recentFiles = result.Items.ToList();
        }
        catch (Exception ex)
        {
            await Notify.ErrorAsync(L["FailedToLoadFiles", ex.Message]);
        }
    }

    private async Task LoadStructures()
    {
        try
        {
            var result = await FileStructureAppService.GetListAsync(new PagedAndSortedResultRequestDto());
            _structures = result.Items.ToList();
        }
        catch (Exception ex)
        {
            await Notify.ErrorAsync(L["FailedToLoadStructures", ex.Message]);
        }
    }

    private async Task RefreshStats()
    {
        try
        {
            var stats = await FileItemAppService.GetStatisticsAsync();
            _statistics.TotalCount = (int)stats.TotalCount;
            _statistics.ImageCount = (int)stats.ImageCount;
            _statistics.VideoCount = (int)stats.VideoCount;
            _statistics.DocumentCount = (int)stats.DocumentCount;
            _statistics.AudioCount = (int)stats.AudioCount;
            _statistics.TotalSize = stats.TotalSize;
        }
        catch (Exception ex)
        {
            await Notify.ErrorAsync(L["FailedToLoadStatistics", ex.Message]);
        }
    }

    private void ViewFile(FileItemDto item)
    {
        _viewingItem = item;
        _viewerModalOpen = true;
    }

    private void DownloadFile(FileItemDto item)
    {
        var url = FileItemUrlProvider.GetDownloadUrl(item.Id, item.StructureBaseUrl, item.StructureIsPublicAccess, item.BlobName, item.TenantId, item.StructureStorageProvider);
        NavigationManager.NavigateTo(url, forceLoad: true);
    }

    private async Task DeleteFile(Guid id)
    {
        try
        {
            await FileItemAppService.DeleteAsync(id);
            await Notify.SuccessAsync(L["FileDeletedSuccessfully"]);
            await LoadData();
        }
        catch (Exception ex)
        {
            await Notify.ErrorAsync(L["FailedToDelete", ex.Message]);
        }
    }

    private static string GetViewerMediaKey(FileItemDto item)
    {
        var ticks = (item.LastModificationTime ?? item.CreationTime).Ticks;
        return $"{item.Id}_{ticks}";
    }

    private string GetDownloadUrl(FileItemDto file)
    {
        return FileItemUrlProvider.GetDownloadUrl(file.Id, file.StructureBaseUrl, file.StructureIsPublicAccess, file.BlobName, file.TenantId, file.StructureStorageProvider);
    }

    private string GetThumbnailUrl(FileItemDto file)
    {
        return FileItemUrlProvider.GetThumbnailUrl(file.Id, (file.LastModificationTime ?? file.CreationTime).Ticks, file.StructureBaseUrl, file.StructureIsPublicAccess, file.ThumbnailBlobName, file.TenantId, file.StructureStorageProvider);
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

    private static IEnumerable<FileType> GetFileTypes(FileType allowedTypes)
    {
        foreach (FileType type in Enum.GetValues<FileType>())
        {
            if (type != FileType.None && allowedTypes.HasFlag(type))
                yield return type;
        }
    }

    private static IEnumerable<string> GetExtensions(string? allowedExtensions)
    {
        if (string.IsNullOrWhiteSpace(allowedExtensions))
            yield break;
        foreach (var ext in allowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = ext.Trim();
            if (trimmed.Length > 0)
                yield return trimmed;
        }
    }

    private string FormatSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

   private bool CanDelete()
   {
        return _canDelete;
   }

    private class FileStatistics
    {
        public int TotalCount { get; set; }
        public int ImageCount { get; set; }
        public int VideoCount { get; set; }
        public int DocumentCount { get; set; }
        public int AudioCount { get; set; }
        public long TotalSize { get; set; }
    }
}
