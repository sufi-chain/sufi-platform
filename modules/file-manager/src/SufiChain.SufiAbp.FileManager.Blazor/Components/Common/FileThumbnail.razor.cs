using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.FileManager.Blazor.Public.Services;
using SufiChain.SufiAbp.FileManager.FileItems;

namespace SufiChain.SufiAbp.FileManager.Blazor.Components.Common;

/// <summary>
/// Thumbnail component for displaying file previews.
/// </summary>
public partial class FileThumbnail : ComponentBase
{
    [Inject]
    protected IFileItemUrlProvider FileItemUrlProvider { get; set; } = default!;

    [Parameter] 
    public FileItemDto? FileItem { get; set; }
    
    [Parameter] 
    public int Width { get; set; } = 150;
    
    [Parameter] 
    public int Height { get; set; } = 150;
    
    [Parameter] 
    public bool Rounded { get; set; } = false;
    
    [Parameter] 
    public bool ShowTitle { get; set; } = false;
    
    [Parameter] 
    public bool ShowBadge { get; set; } = false;
    
    [Parameter] 
    public string? BadgeText { get; set; }
    
    [Parameter] 
    public string BadgeClass { get; set; } = "bg-primary";
    
    [Parameter] 
    public string CssClass { get; set; } = "";
    
    [Parameter] 
    public string Class
    {
        get => CssClass;
        set => CssClass = value;
    }
    
    [Parameter] 
    public string Style { get; set; } = "";

    private string GetThumbnailUrl()
    {
        if (FileItem == null)
            return "#";

        // If has thumbnail, use it
        if (!string.IsNullOrEmpty(FileItem.ThumbnailBlobName))
        {
            return FileItemUrlProvider.GetThumbnailUrl(FileItem.Id, (FileItem.LastModificationTime ?? FileItem.CreationTime).Ticks, FileItem.StructureBaseUrl, FileItem.StructureIsPublicAccess, FileItem.ThumbnailBlobName, FileItem.TenantId, FileItem.StructureStorageProvider);
        }

        // Otherwise use original
        return FileItemUrlProvider.GetDownloadUrl(FileItem.Id, FileItem.StructureBaseUrl, FileItem.StructureIsPublicAccess, FileItem.BlobName, FileItem.TenantId, FileItem.StructureStorageProvider);
    }
}
