using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.FileManager.Permissions;
using Volo.Abp;

namespace SufiChain.SufiPlatform.FileManager.Controllers.Integration;

[Authorize]
[RemoteService(Name = FileManagerRemoteServiceConsts.RemoteServiceName)]
[Area(FileManagerRemoteServiceConsts.ModuleName)]
[ControllerName("FileStorageIntegration")]
[Route("integration-api/file-manager/files")]
public class FileStorageIntegrationController : SufiControllerBase, IFileStorageIntegrationService
{
    protected IFileStorageIntegrationService FileStorageIntegrationService { get; }

    public FileStorageIntegrationController(IFileStorageIntegrationService fileStorageIntegrationService)
    {
        FileStorageIntegrationService = fileStorageIntegrationService;
    }

    [HttpPost]
    public virtual Task<FileReferenceDto> UploadAsync([FromBody] FileUploadRequest input)
    {
        return FileStorageIntegrationService.UploadAsync(input);
    }

    [HttpGet]
    [Route("{id}")]
    [Authorize(FileManagerPermissions.FileItems.Default)]
    public virtual Task<FileReferenceDto> GetAsync(Guid id)
    {
        return FileStorageIntegrationService.GetAsync(id);
    }

    [HttpGet]
    [Route("{id}/content")]
    [Authorize(FileManagerPermissions.FileItems.Default)]
    public virtual Task<FileContentBytesDto> GetContentAsync(Guid id)
    {
        return FileStorageIntegrationService.GetContentAsync(id);
    }

    [HttpGet]
    [Route("{id}/access-token")]
    [Authorize(FileManagerPermissions.FileItems.Default)]
    public virtual Task<string> GetAccessTokenAsync(Guid id)
    {
        return FileStorageIntegrationService.GetAccessTokenAsync(id);
    }

    [HttpDelete]
    [Route("{id}")]
    [Authorize(FileManagerPermissions.FileItems.Default)]
    public virtual Task DeleteAsync(Guid id)
    {
        return FileStorageIntegrationService.DeleteAsync(id);
    }
}
