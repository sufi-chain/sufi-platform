using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.FileManager.Blazor.Public.Services;

namespace SufiChain.SufiAbp.FileManager.Blazor.Public.Components;

/// <summary>
/// A list of downloadable file attachments.
/// </summary>
public partial class FileAttachmentList : ComponentBase
{
    [Inject]
    protected IFilePublicUrlResolver UrlResolver { get; set; } = default!;

    /// <summary>
    /// The list of file IDs to display.
    /// </summary>
    [Parameter]
    public IEnumerable<Guid> FileIds { get; set; } = Enumerable.Empty<Guid>();

    /// <summary>
    /// Optional title for the attachment list.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Whether to use compact display mode.
    /// </summary>
    [Parameter]
    public bool Compact { get; set; } = false;

    /// <summary>
    /// Whether to show file sizes.
    /// </summary>
    [Parameter]
    public bool ShowFileSize { get; set; } = true;

    /// <summary>
    /// Whether to show a separate download button.
    /// </summary>
    [Parameter]
    public bool ShowDownloadButton { get; set; } = true;

    /// <summary>
    /// Whether to show total size summary.
    /// </summary>
    [Parameter]
    public bool ShowTotalSize { get; set; } = true;

    /// <summary>
    /// CSS class for the container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Template to show while loading.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// Template to show when empty.
    /// </summary>
    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    private List<FilePublicInfo> _fileInfos = new();
    private bool _isLoading = true;
    private IEnumerable<Guid> _lastFileIds = Enumerable.Empty<Guid>();

    protected override async Task OnParametersSetAsync()
    {
        if (!FileIds.SequenceEqual(_lastFileIds))
        {
            _lastFileIds = FileIds.ToList();
            await LoadFileInfosAsync();
        }
    }

    private async Task LoadFileInfosAsync()
    {
        _isLoading = true;
        StateHasChanged();

        _fileInfos.Clear();

        foreach (var fileId in FileIds)
        {
            var info = await UrlResolver.GetFilePublicInfoAsync(fileId);
            if (info != null && !string.IsNullOrEmpty(info.DownloadUrl))
            {
                _fileInfos.Add(info);
            }
        }

        _isLoading = false;
    }

    private static string GetFileIcon(string mimeType)
    {
        return mimeType.ToLowerInvariant() switch
        {
            var m when m.StartsWith("image/") => "file-image",
            var m when m.StartsWith("video/") => "file-video",
            var m when m.StartsWith("audio/") => "file-audio",
            var m when m.Contains("pdf") => "file-pdf",
            var m when m.Contains("word") || m.Contains("msword") || m.Contains("opendocument.text") => "file-doc",
            var m when m.Contains("excel") || m.Contains("spreadsheet") || m.Contains("opendocument.spreadsheet") => "file-excel",
            var m when m.Contains("powerpoint") || m.Contains("presentation") || m.Contains("opendocument.presentation") => "file-ppt",
            var m when m.Contains("csv") => "file-csv",
            var m when m.Contains("json") => "file-json",
            var m when m.Contains("xml") => "file-xml",
            var m when m.Contains("zip") || m.Contains("rar") || m.Contains("archive") || m.Contains("x-7z") => "file-archive",
            var m when m.Contains("text") => "file-text",
            _ => "file"
        };
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }
}
