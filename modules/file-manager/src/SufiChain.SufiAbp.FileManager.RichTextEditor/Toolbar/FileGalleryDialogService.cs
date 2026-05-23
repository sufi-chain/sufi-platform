using System;
using System.Threading.Tasks;
using SufiChain.SufiAbp.FileManager.Blazor.Public.Services;
using SufiChain.SufiAbp.FileManager.FileItems;

namespace SufiChain.SufiAbp.FileManager.RichTextEditor.Toolbar;

/// <summary>
/// Service for managing file gallery dialogs.
/// This service uses a callback-based approach for Blazor component communication.
/// </summary>
public class FileGalleryDialogService : IFileGalleryDialogService
{
    private readonly IFileItemUrlProvider _fileItemUrlProvider;
    private TaskCompletionSource<FileGalleryResult?>? _imageCompletionSource;
    private TaskCompletionSource<FileGalleryResult?>? _fileCompletionSource;
    private int _hostRegistrationCount;

    /// <inheritdoc />
    public bool IsHostRegistered => _hostRegistrationCount > 0;

    /// <summary>
    /// Called by FileGalleryHost when it mounts. Registers this instance as the active host.
    /// </summary>
    public void RegisterHost()
    {
        _hostRegistrationCount++;
    }

    /// <summary>
    /// Called by FileGalleryHost when it disposes. Unregisters this instance.
    /// </summary>
    public void UnregisterHost()
    {
        if (_hostRegistrationCount > 0)
            _hostRegistrationCount--;
    }

    public FileGalleryDialogService(IFileItemUrlProvider fileItemUrlProvider)
    {
        _fileItemUrlProvider = fileItemUrlProvider;
    }

    /// <summary>
    /// Event raised when the image gallery should be shown.
    /// </summary>
    public event Action? OnShowImageGallery;

    /// <summary>
    /// Event raised when the file gallery should be shown.
    /// </summary>
    public event Action? OnShowFileGallery;

    /// <summary>
    /// Whether the image gallery dialog should be open.
    /// </summary>
    public bool IsImageGalleryOpen { get; private set; }

    /// <summary>
    /// Whether the file gallery dialog should be open.
    /// </summary>
    public bool IsFileGalleryOpen { get; private set; }

    public async Task<FileGalleryResult?> ShowImageGalleryAsync()
    {
        _imageCompletionSource = new TaskCompletionSource<FileGalleryResult?>();
        IsImageGalleryOpen = true;
        OnShowImageGallery?.Invoke();

        try
        {
            return await _imageCompletionSource.Task;
        }
        finally
        {
            IsImageGalleryOpen = false;
        }
    }

    public async Task<FileGalleryResult?> ShowFileGalleryAsync()
    {
        _fileCompletionSource = new TaskCompletionSource<FileGalleryResult?>();
        IsFileGalleryOpen = true;
        OnShowFileGallery?.Invoke();

        try
        {
            return await _fileCompletionSource.Task;
        }
        finally
        {
            IsFileGalleryOpen = false;
        }
    }

    /// <summary>
    /// Called by the dialog component when an image is selected.
    /// </summary>
    public void CompleteImageSelection(FileItemDto? fileItem)
    {
        IsImageGalleryOpen = false;

        if (fileItem != null)
        {
            _imageCompletionSource?.TrySetResult(new FileGalleryResult
            {
                FileId = fileItem.Id,
                Url = _fileItemUrlProvider.GetDownloadUrl(fileItem.Id, fileItem.StructureBaseUrl, fileItem.StructureIsPublicAccess, fileItem.BlobName, fileItem.TenantId, fileItem.StructureStorageProvider),
                FileName = fileItem.OriginalName,
                Alt = fileItem.Alt ?? fileItem.Name,
                MimeType = fileItem.MimeType
            });
        }
        else
        {
            _imageCompletionSource?.TrySetResult(null);
        }
    }

    /// <summary>
    /// Called by the dialog component when a file is selected.
    /// </summary>
    public void CompleteFileSelection(FileItemDto? fileItem)
    {
        IsFileGalleryOpen = false;

        if (fileItem != null)
        {
            _fileCompletionSource?.TrySetResult(new FileGalleryResult
            {
                FileId = fileItem.Id,
                Url = _fileItemUrlProvider.GetDownloadUrl(fileItem.Id, fileItem.StructureBaseUrl, fileItem.StructureIsPublicAccess, fileItem.BlobName, fileItem.TenantId, fileItem.StructureStorageProvider),
                FileName = fileItem.OriginalName,
                Alt = fileItem.Alt ?? fileItem.Name,
                MimeType = fileItem.MimeType
            });
        }
        else
        {
            _fileCompletionSource?.TrySetResult(null);
        }
    }
}
