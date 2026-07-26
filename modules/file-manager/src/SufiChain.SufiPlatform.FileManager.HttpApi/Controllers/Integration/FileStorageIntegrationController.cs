using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiPlatform.FileManager.Controllers.Integration;

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
    public virtual Task<FileReferenceDto> GetAsync(Guid id)
    {
        return FileStorageIntegrationService.GetAsync(id);
    }

    [HttpGet]
    [Route("{id}/content")]
    public virtual Task<FileContentBytesDto> GetContentAsync(Guid id)
    {
        return FileStorageIntegrationService.GetContentAsync(id);
    }

    [HttpGet]
    [Route("{id}/access-token")]
    public virtual Task<string> GetAccessTokenAsync(Guid id)
    {
        return FileStorageIntegrationService.GetAccessTokenAsync(id);
    }

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return FileStorageIntegrationService.DeleteAsync(id);
    }
}
