using System;
using System.Threading.Tasks;
using SufiChain.SufiAbp.FileManager.Blazor.Public.Services;
using SufiChain.SufiAbp.FileManager.FileItems;

namespace SufiChain.SufiAbp.FileManager.Blazor.Public.Editors;

public class FileGalleryDialogService : IFileGalleryDialogService
{
    /// <summary>
    /// Cascading parameter name for sharing one dialog service instance across
    /// <see cref="FileGalleryHost"/> and gallery consumers on the same page.
    /// </summary>
    public const string CascadeName = "FileGalleryDialogService";

    private readonly IFileItemUrlProvider _fileItemUrlProvider;
    private TaskCompletionSource<FileGalleryResult?>? _imageCompletionSource;
    private TaskCompletionSource<FileGalleryResult?>? _fileCompletionSource;
    private int _hostRegistrationCount;

    public FileGalleryDialogService(IFileItemUrlProvider fileItemUrlProvider)
    {
        _fileItemUrlProvider = fileItemUrlProvider;
    }

    public bool IsHostRegistered => _hostRegistrationCount > 0;
    public event Action? OnShowImageGallery;
    public event Action? OnShowFileGallery;

    public void RegisterHost()
    {
        _hostRegistrationCount++;
    }

    public void UnregisterHost()
    {
        if (_hostRegistrationCount > 0)
        {
            _hostRegistrationCount--;
        }
    }

    public async Task<FileGalleryResult?> ShowImageGalleryAsync()
    {
        _imageCompletionSource = new TaskCompletionSource<FileGalleryResult?>();
        OnShowImageGallery?.Invoke();
        return await _imageCompletionSource.Task;
    }

    public async Task<FileGalleryResult?> ShowFileGalleryAsync()
    {
        _fileCompletionSource = new TaskCompletionSource<FileGalleryResult?>();
        OnShowFileGallery?.Invoke();
        return await _fileCompletionSource.Task;
    }

    public void CompleteImageSelection(FileItemDto? fileItem)
    {
        _imageCompletionSource?.TrySetResult(Map(fileItem));
    }

    public void CompleteFileSelection(FileItemDto? fileItem)
    {
        _fileCompletionSource?.TrySetResult(Map(fileItem));
    }

    private FileGalleryResult? Map(FileItemDto? fileItem)
    {
        if (fileItem == null)
        {
            return null;
        }

        return new FileGalleryResult
        {
            FileId = fileItem.Id,
            Url = _fileItemUrlProvider.GetDownloadUrl(fileItem.Id, fileItem.StructureBaseUrl, fileItem.StructureIsPublicAccess, fileItem.BlobName, fileItem.TenantId, fileItem.StructureStorageProvider),
            FileName = fileItem.OriginalName,
            Alt = fileItem.Alt ?? fileItem.Name,
            MimeType = fileItem.MimeType
        };
    }
}
