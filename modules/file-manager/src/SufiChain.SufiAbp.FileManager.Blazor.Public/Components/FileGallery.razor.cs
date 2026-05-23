using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SufiChain.SufiAbp.FileManager.Blazor.Public.Services;

namespace SufiChain.SufiAbp.FileManager.Blazor.Public.Components;

/// <summary>
/// A responsive image gallery with lightbox support.
/// </summary>
public partial class FileGallery : ComponentBase
{
    [Inject]
    protected IFilePublicUrlResolver UrlResolver { get; set; } = default!;

    [Inject]
    protected IJSRuntime JS { get; set; } = default!;

    /// <summary>
    /// The list of file IDs to display in the gallery.
    /// </summary>
    [Parameter]
    public IEnumerable<Guid> FileIds { get; set; } = Enumerable.Empty<Guid>();

    /// <summary>
    /// Size for thumbnail images in the grid.
    /// </summary>
    [Parameter]
    public FileImageSize ThumbnailSize { get; set; } = FileImageSize.Small;

    /// <summary>
    /// Number of columns in the grid.
    /// </summary>
    [Parameter]
    public int Columns { get; set; } = 4;

    /// <summary>
    /// Whether to enable lightbox viewing.
    /// </summary>
    [Parameter]
    public bool EnableLightbox { get; set; } = true;

    /// <summary>
    /// CSS class for the gallery container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Inline style for the gallery container.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Callback when an image is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<Guid> OnImageClick { get; set; }

    private List<FilePublicInfo> _fileInfos = new();
    private bool _lightboxOpen;
    private int _currentIndex = -1;
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
        _fileInfos.Clear();

        foreach (var fileId in FileIds)
        {
            var info = await UrlResolver.GetFilePublicInfoAsync(fileId);
            if (info != null && info.IsImage)
            {
                _fileInfos.Add(info);
            }
        }

        StateHasChanged();
    }

    private async Task OpenLightbox(int index)
    {
        if (index >= 0 && index < _fileInfos.Count)
        {
            await OnImageClick.InvokeAsync(_fileInfos[index].Id);
        }

        if (EnableLightbox)
        {
            _currentIndex = index;
            _lightboxOpen = true;
        }
    }

    private void CloseLightbox()
    {
        _lightboxOpen = false;
        _currentIndex = -1;
    }

    private void NextImage()
    {
        if (_currentIndex < _fileInfos.Count - 1)
        {
            _currentIndex++;
        }
    }

    private void PreviousImage()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
        }
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "Escape":
                CloseLightbox();
                break;
            case "ArrowRight":
                NextImage();
                break;
            case "ArrowLeft":
                PreviousImage();
                break;
        }
    }
}
