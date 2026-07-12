using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Components;

/// <summary>
/// A responsive video player for public file display.
/// </summary>
public partial class FileVideo : ComponentBase
{
    [Inject]
    protected IFilePublicUrlResolver UrlResolver { get; set; } = default!;

    /// <summary>
    /// The file ID to display.
    /// </summary>
    [Parameter]
    public Guid FileId { get; set; }

    /// <summary>
    /// Whether to show video controls.
    /// </summary>
    [Parameter]
    public bool Controls { get; set; } = true;

    /// <summary>
    /// Whether to autoplay the video.
    /// </summary>
    [Parameter]
    public bool Autoplay { get; set; } = false;

    /// <summary>
    /// Whether to mute the video.
    /// </summary>
    [Parameter]
    public bool Muted { get; set; } = false;

    /// <summary>
    /// Whether to loop the video.
    /// </summary>
    [Parameter]
    public bool Loop { get; set; } = false;

    /// <summary>
    /// Whether to play inline on mobile devices.
    /// </summary>
    [Parameter]
    public bool Playsinline { get; set; } = true;

    /// <summary>
    /// Preload behavior: "auto", "metadata", or "none".
    /// </summary>
    [Parameter]
    public string Preload { get; set; } = "metadata";

    /// <summary>
    /// Custom poster image URL.
    /// </summary>
    [Parameter]
    public string? Poster { get; set; }

    /// <summary>
    /// Whether to show a download link below the video.
    /// </summary>
    [Parameter]
    public bool ShowDownloadLink { get; set; } = false;

    /// <summary>
    /// CSS class for the video container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Inline style for the video container.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Video width in pixels. 0 means auto.
    /// </summary>
    [Parameter]
    public int Width { get; set; }

    /// <summary>
    /// Video height in pixels. 0 means auto.
    /// </summary>
    [Parameter]
    public int Height { get; set; }

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
            await LoadVideoInfoAsync();
        }
    }

    private async Task LoadVideoInfoAsync()
    {
        _isLoading = true;
        _hasError = false;
        StateHasChanged();

        try
        {
            _fileInfo = await UrlResolver.GetFilePublicInfoAsync(FileId);
            if (_fileInfo == null || !_fileInfo.IsVideo)
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

    private string GetContainerStyle()
    {
        var styles = new List<string>();
        if (Width > 0) styles.Add($"width: {Width}px");
        if (Height > 0) styles.Add($"height: {Height}px");
        if (!string.IsNullOrEmpty(Style)) styles.Add(Style);
        return string.Join("; ", styles);
    }
}
