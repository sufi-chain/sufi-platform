using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;
using SufiChain.SufiPlatform.FileManager.FileItems;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Components.Common;

/// <summary>
/// Card component for displaying file item information with optional actions.
/// </summary>
public partial class FileCard : ComponentBase
{
    [Inject]
    protected IFileItemUrlProvider FileItemUrlProvider { get; set; } = default!;

    [Parameter, EditorRequired] 
    public FileItemDto FileItem { get; set; } = default!;
    
    [Parameter] 
    public bool ShowDetails { get; set; } = true;
    
    [Parameter] 
    public bool ShowMetadata { get; set; } = true;
    
    [Parameter] 
    public bool ShowActions { get; set; } = true;
    
    [Parameter] 
    public bool Selectable { get; set; } = false;
    
    [Parameter] 
    public bool IsSelected { get; set; } = false;
    
    [Parameter] 
    public EventCallback<FileItemDto> OnView { get; set; }
    
    [Parameter] 
    public EventCallback<FileItemDto> OnEdit { get; set; }
    
    [Parameter] 
    public EventCallback<FileItemDto> OnDelete { get; set; }
    
    [Parameter] 
    public EventCallback<FileItemDto> OnSelect { get; set; }
    
    [Parameter] 
    public string CssClass { get; set; } = "";
    
    [Parameter] 
    public string Style { get; set; } = "";

    private string GetImageUrl()
    {
        if (!string.IsNullOrEmpty(FileItem.ThumbnailBlobName))
        {
            return FileItemUrlProvider.GetThumbnailUrl(FileItem.Id, (FileItem.LastModificationTime ?? FileItem.CreationTime).Ticks, FileItem.StructureBaseUrl, FileItem.StructureIsPublicAccess, FileItem.ThumbnailBlobName, FileItem.TenantId, FileItem.StructureStorageProvider);
        }
        return FileItemUrlProvider.GetDownloadUrl(FileItem.Id, FileItem.StructureBaseUrl, FileItem.StructureIsPublicAccess, FileItem.BlobName, FileItem.TenantId, FileItem.StructureStorageProvider);
    }

    private string FormatFileSize(long bytes)
    {
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

    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return duration.ToString(@"hh\:mm\:ss");
        }
        return duration.ToString(@"mm\:ss");
    }
}
