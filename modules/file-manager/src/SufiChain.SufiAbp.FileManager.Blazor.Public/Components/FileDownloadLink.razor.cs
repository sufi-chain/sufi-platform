using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SufiChain.SufiAbp.FileManager.Blazor.Public.Services;

namespace SufiChain.SufiAbp.FileManager.Blazor.Public.Components;

/// <summary>
/// A download button/link for files.
/// </summary>
public partial class FileDownloadLink : ComponentBase
{
    [Inject]
    protected IFilePublicUrlResolver UrlResolver { get; set; } = default!;

    /// <summary>
    /// The file ID to provide download for.
    /// </summary>
    [Parameter]
    public Guid FileId { get; set; }

    /// <summary>
    /// Whether to render as a button style (default) or link style.
    /// </summary>
    [Parameter]
    public bool AsButton { get; set; } = true;

    /// <summary>
    /// Whether to show the file icon.
    /// </summary>
    [Parameter]
    public bool ShowIcon { get; set; } = true;

    /// <summary>
    /// Whether to show the file size.
    /// </summary>
    [Parameter]
    public bool ShowSize { get; set; } = true;

    /// <summary>
    /// Custom text to display. If not set, uses the file name.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// CSS class for the link/button.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Child content to customize the link content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Template to show while loading.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// Template to show on error.
    /// </summary>
    [Parameter]
    public RenderFragment? ErrorTemplate { get; set; }

    private FilePublicInfo? _fileInfo;
    private bool _isLoading = true;
    private bool _hasError;
    private Guid _lastFileId;

    protected override async Task OnParametersSetAsync()
    {
        if (FileId != _lastFileId && FileId != Guid.Empty)
        {
            _lastFileId = FileId;
            await LoadFileInfoAsync();
        }
    }

    private async Task LoadFileInfoAsync()
    {
        _isLoading = true;
        _hasError = false;
        StateHasChanged();

        try
        {
            _fileInfo = await UrlResolver.GetFilePublicInfoAsync(FileId);
            if (_fileInfo == null || string.IsNullOrEmpty(_fileInfo.DownloadUrl))
            {
                _hasError = true;
            }
        }
        catch
        {
            _hasError = true;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private RenderFragment RenderContent() => builder =>
    {
        if (ChildContent != null)
        {
            builder.AddContent(0, ChildContent);
        }
        else
        {
            var seq = 0;
            if (ShowIcon)
            {
                builder.OpenElement(seq++, "span");
                builder.AddAttribute(seq++, "class", "file-download-link__icon");
                builder.AddContent(seq++, GetFileIcon());
                builder.CloseElement();
            }
            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++, "class", "file-download-link__text");
            builder.AddContent(seq++, Text ?? _fileInfo?.FileName ?? "Download");
            builder.CloseElement();
            if (ShowSize && _fileInfo != null)
            {
                builder.OpenElement(seq++, "span");
                builder.AddAttribute(seq++, "class", "file-download-link__size");
                builder.AddContent(seq++, $"({FormatFileSize(_fileInfo.Size)})");
                builder.CloseElement();
            }
        }
    };

    private string GetFileIcon()
    {
        if (_fileInfo == null) return "file";

        return _fileInfo.MimeType.ToLowerInvariant() switch
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
