using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Components;

/// <summary>
/// A responsive, SSR-safe image component for public file display.
/// </summary>
public partial class FileImage : ComponentBase
{
    [Inject]
    protected IFilePublicUrlResolver UrlResolver { get; set; } = default!;

    /// <summary>
    /// The file ID to display.
    /// </summary>
    [Parameter]
    public Guid FileId { get; set; }

    /// <summary>
    /// The preferred image size to display.
    /// </summary>
    [Parameter]
    public FileImageSize Size { get; set; } = FileImageSize.Medium;

    /// <summary>
    /// Alt text for the image. If not provided, uses file metadata.
    /// </summary>
    [Parameter]
    public string? Alt { get; set; }

    /// <summary>
    /// Whether to lazy load the image.
    /// </summary>
    [Parameter]
    public bool LazyLoad { get; set; } = true;

    /// <summary>
    /// CSS class to apply to the image.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Inline style to apply to the image.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Image width in pixels. 0 means auto.
    /// </summary>
    [Parameter]
    public int Width { get; set; }

    /// <summary>
    /// Image height in pixels. 0 means auto.
    /// </summary>
    [Parameter]
    public int Height { get; set; }

    /// <summary>
    /// The sizes attribute for responsive images.
    /// </summary>
    [Parameter]
    public string Sizes { get; set; } = "(max-width: 320px) 280px, (max-width: 640px) 600px, 1024px";

    /// <summary>
    /// Whether to generate srcset for responsive images.
    /// </summary>
    [Parameter]
    public bool UseSrcSet { get; set; } = true;

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

    /// <summary>
    /// Callback when the image loads successfully.
    /// </summary>
    [Parameter]
    public EventCallback OnLoad { get; set; }

    /// <summary>
    /// Callback when the image fails to load.
    /// </summary>
    [Parameter]
    public EventCallback OnError { get; set; }

    private string? _imageUrl;
    private string? _srcset;
    private string? _altFromMetadata;
    private bool _isLoading = true;
    private bool _hasError;
    private Guid _lastFileId;

    protected override async Task OnParametersSetAsync()
    {
        if (FileId != _lastFileId && FileId != Guid.Empty)
        {
            _lastFileId = FileId;
            await LoadImageAsync();
        }
    }

    private async Task LoadImageAsync()
    {
        _isLoading = true;
        _hasError = false;
        StateHasChanged();

        try
        {
            var fileInfo = await UrlResolver.GetFilePublicInfoAsync(FileId);
            if (fileInfo == null || !fileInfo.IsImage)
            {
                _hasError = true;
                await OnError.InvokeAsync();
                return;
            }

            _altFromMetadata = fileInfo.Alt ?? fileInfo.FileName;

            // Select URL based on requested size
            _imageUrl = Size switch
            {
                FileImageSize.Thumbnail => fileInfo.ThumbnailUrl ?? fileInfo.DownloadUrl,
                FileImageSize.Small => fileInfo.SizeVariants.GetValueOrDefault(FileImageSize.Small) ?? fileInfo.ThumbnailUrl ?? fileInfo.DownloadUrl,
                FileImageSize.Medium => fileInfo.SizeVariants.GetValueOrDefault(FileImageSize.Medium) ?? fileInfo.DownloadUrl,
                FileImageSize.Large => fileInfo.SizeVariants.GetValueOrDefault(FileImageSize.Large) ?? fileInfo.DownloadUrl,
                _ => fileInfo.DownloadUrl
            };

            // Build srcset if enabled and variants are available
            if (UseSrcSet && fileInfo.SizeVariants.Count > 0)
            {
                var srcsetParts = new List<string>();
                foreach (var variant in fileInfo.SizeVariants)
                {
                    if (!string.IsNullOrEmpty(variant.Value))
                    {
                        var width = variant.Key == FileImageSize.Original ? "2048w" : $"{(int)variant.Key}w";
                        srcsetParts.Add($"{variant.Value} {width}");
                    }
                }
                _srcset = srcsetParts.Count > 0 ? string.Join(", ", srcsetParts) : null;
            }

            await OnLoad.InvokeAsync();
        }
        catch
        {
            _hasError = true;
            await OnError.InvokeAsync();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private string GetAltText()
    {
        return Alt ?? _altFromMetadata ?? "Image";
    }

    private string GetContainerStyle()
    {
        var styles = new List<string>();
        if (Width > 0) styles.Add($"width: {Width}px");
        if (Height > 0) styles.Add($"height: {Height}px");
        if (!string.IsNullOrEmpty(Style)) styles.Add(Style);
        return string.Join("; ", styles);
    }

    private async Task HandleImageError()
    {
        _hasError = true;
        await OnError.InvokeAsync();
        StateHasChanged();
    }
}
