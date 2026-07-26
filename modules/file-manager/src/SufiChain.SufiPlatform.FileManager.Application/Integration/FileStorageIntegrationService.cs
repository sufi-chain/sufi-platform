using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.FileManager.FileItems;

namespace SufiChain.SufiPlatform.FileManager.Integration;

/// <summary>
/// Cross-module file storage integration service.
/// </summary>
public class FileStorageIntegrationService : SufiApplicationService, IFileStorageIntegrationService
{
    protected IFileItemAppService FileItemAppService { get; }

    protected IFileAccessTokenService FileAccessTokenService { get; }

    public FileStorageIntegrationService(
        IFileItemAppService fileItemAppService,
        IFileAccessTokenService fileAccessTokenService)
    {
        FileItemAppService = fileItemAppService;
        FileAccessTokenService = fileAccessTokenService;
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
        var fileItem = await FileItemAppService.GetAsync(id);
        return MapToFileReferenceDto(fileItem);
    }

    public virtual async Task<FileContentBytesDto> GetContentAsync(Guid id)
    {
        var result = await FileItemAppService.GetDownloadContentAsync(id, token: null);
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
        string? accessToken = null;
        if (FileAccessTokenService.TryGenerateToken(fileItem.Id, out var token))
        {
            accessToken = token;
        }

        return new FileReferenceDto
        {
            Id = fileItem.Id,
            FileName = fileItem.OriginalName,
            MimeType = fileItem.MimeType,
            SizeInBytes = fileItem.Size,
            AccessToken = accessToken,
            StructureKey = fileItem.StructureKey,
            EntityType = fileItem.EntityType,
            EntityId = fileItem.EntityId,
            TenantId = fileItem.TenantId
        };
    }
}
