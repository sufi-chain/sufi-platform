using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.FileItems;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Editors;

/// <summary>
/// Hosts file gallery dialogs and wires them to <see cref="FileGalleryDialogService"/>.
/// Use a cascading value (see <see cref="FileGalleryDialogService.CascadeName"/>) from a parent
/// settings/page component so consumers share the same service instance as this host.
/// </summary>
public partial class FileGalleryHost : IDisposable
{
    [CascadingParameter(Name = FileGalleryDialogService.CascadeName)]
    public FileGalleryDialogService? CascadedDialogService { get; set; }

    [Inject]
    public FileGalleryDialogService InjectedDialogService { get; set; } = default!;

    private FileGalleryDialogService DialogService =>
        CascadedDialogService ?? InjectedDialogService;

    private bool _isImageGalleryOpen;
    private bool _isFileGalleryOpen;

    protected override void OnInitialized()
    {
        DialogService.RegisterHost();
        DialogService.OnShowImageGallery += ShowImageGallery;
        DialogService.OnShowFileGallery += ShowFileGallery;
    }

    private void ShowImageGallery()
    {
        _isImageGalleryOpen = true;
        _ = InvokeAsync(StateHasChanged);
    }

    private void ShowFileGallery()
    {
        _isFileGalleryOpen = true;
        _ = InvokeAsync(StateHasChanged);
    }

    private Task OnImageGalleryOpenChangedAsync(bool open)
    {
        _isImageGalleryOpen = open;
        return InvokeAsync(StateHasChanged);
    }

    private Task OnFileGalleryOpenChangedAsync(bool open)
    {
        _isFileGalleryOpen = open;
        return InvokeAsync(StateHasChanged);
    }

    private void OnImageSelected(FileItemDto? fileItem)
    {
        _isImageGalleryOpen = false;
        DialogService.CompleteImageSelection(fileItem);
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnFileSelected(FileItemDto? fileItem)
    {
        _isFileGalleryOpen = false;
        DialogService.CompleteFileSelection(fileItem);
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        DialogService.OnShowImageGallery -= ShowImageGallery;
        DialogService.OnShowFileGallery -= ShowFileGallery;
        DialogService.UnregisterHost();
    }
}
