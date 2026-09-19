using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.FileManager.FileItems;

namespace SufiChain.SufiPlatform.FileManager.Integration;

/// <summary>
/// Cross-module file storage integration service.
/// Get and content reads use the File Manager repository and blob access path.
/// They do not require <c>SufiFileManager.FileItems</c>.
/// Callers must authorize the file in their own domain.
/// </summary>
public class FileStorageIntegrationService : SufiApplicationService, IFileStorageIntegrationService
{
    protected IFileItemAppService FileItemAppService { get; }

    protected IFileAccessTokenService FileAccessTokenService { get; }

    protected IFileItemRepository FileItemRepository { get; }

    protected FileItemBlobAccessService BlobAccessService { get; }

    public FileStorageIntegrationService(
        IFileItemAppService fileItemAppService,
        IFileAccessTokenService fileAccessTokenService,
        IFileItemRepository fileItemRepository,
        FileItemBlobAccessService blobAccessService)
    {
        FileItemAppService = fileItemAppService;
        FileAccessTokenService = fileAccessTokenService;
        FileItemRepository = fileItemRepository;
        BlobAccessService = blobAccessService;
    }

    public virtual async Task<FileReferenceDto> UploadAsync(FileUploadRequest input)
    {
        var uploadInput = new UploadFileInput
        {
            FileName = input.FileName,
            Content = input.Content,
            MimeType = input.MimeType,
            StructureKey = input.StructureKey,
            EntityType = input.EntityType,
            EntityId = input.EntityId,
            FolderId = input.FolderId,
            FolderPath = input.FolderPath,
            AutoConfirm = input.AutoConfirm,
            Alt = input.Alt
        };

        var fileItem = await FileItemAppService.UploadAsync(uploadInput);
        return MapToFileReferenceDto(fileItem);
    }

    public virtual async Task<FileReferenceDto> GetAsync(Guid id)
    {
        var fileItem = await FileItemRepository.GetAsync(id, cancellationToken: default);
        return MapToFileReferenceDto(fileItem);
    }

    public virtual async Task<FileContentBytesDto> GetContentAsync(Guid id)
    {
        var result = await BlobAccessService.GetContentForIntegrationAsync(id);
        if (result.IsForbidden)
        {
            throw new Volo.Abp.Authorization.AbpAuthorizationException(
                $"Access to file '{id}' is forbidden.");
        }

        if (result.Content == null)
        {
            throw new Volo.Abp.BusinessException("SufiFileManager:FileContentNotFound")
                .WithData("FileId", id);
        }

        return new FileContentBytesDto
        {
            Id = id,
            Content = result.Content.Content,
            FileName = result.Content.FileName,
            MimeType = result.Content.MimeType
        };
    }

    public virtual Task<string> GetAccessTokenAsync(Guid id)
    {
        return Task.FromResult(FileAccessTokenService.GenerateToken(id));
    }

    public virtual Task DeleteAsync(Guid id)
    {
        return FileItemAppService.DeleteAsync(id);
    }

    protected virtual FileReferenceDto MapToFileReferenceDto(FileItemDto fileItem)
    {
        return MapToFileReferenceDto(
            fileItem.Id,
            fileItem.OriginalName,
            fileItem.MimeType,
            fileItem.Size,
            fileItem.StructureKey,
            fileItem.EntityType,
            fileItem.EntityId,
            fileItem.TenantId);
    }

    protected virtual FileReferenceDto MapToFileReferenceDto(FileItem fileItem)
    {
        return MapToFileReferenceDto(
            fileItem.Id,
            fileItem.OriginalName,
            fileItem.MimeType,
            fileItem.Size,
            fileItem.StructureKey,
            fileItem.EntityType,
            fileItem.EntityId,
            fileItem.TenantId);
    }

    protected virtual FileReferenceDto MapToFileReferenceDto(
        Guid id,
        string fileName,
        string mimeType,
        long sizeInBytes,
        string? structureKey,
        string? entityType,
        Guid? entityId,
        Guid? tenantId)
    {
        string? accessToken = null;
        if (FileAccessTokenService.TryGenerateToken(id, out var token))
        {
            accessToken = token;
        }

        return new FileReferenceDto
        {
            Id = id,
            FileName = fileName,
            MimeType = mimeType,
            SizeInBytes = sizeInBytes,
            AccessToken = accessToken,
            StructureKey = structureKey,
            EntityType = entityType,
            EntityId = entityId,
            TenantId = tenantId
        };
    }
}
