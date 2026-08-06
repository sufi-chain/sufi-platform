using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiPlatform.FileManager.Controllers;

[Area(FileManagerRemoteServiceConsts.ModuleName)]
[RemoteService(Name = FileManagerRemoteServiceConsts.RemoteServiceName)]
[Route("api/file-manager/file-structures")]
public class FileStructureController : SufiControllerBase, IFileStructureAppService
{
    private readonly IFileStructureAppService _fileStructureAppService;

    public FileStructureController(IFileStructureAppService fileStructureAppService)
    {
        _fileStructureAppService = fileStructureAppService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<FileStructureDto>> GetListAsync([FromQuery] PagedAndSortedResultRequestDto input)
    {
        return _fileStructureAppService.GetListAsync(input);
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<FileStructureDto> GetAsync(Guid id)
    {
        return _fileStructureAppService.GetAsync(id);
    }

    [HttpGet]
    [Route("by-key/{key}")]
    public virtual Task<FileStructureDto> GetByKeyAsync(string key)
    {
        return _fileStructureAppService.GetByKeyAsync(key);
    }

    [HttpGet]
    [Route("exists/{key}")]
    public virtual Task<bool> ExistsAsync(string key)
    {
        return _fileStructureAppService.ExistsAsync(key);
    }

    [HttpPost]
    public virtual Task<FileStructureDto> CreateAsync(CreateUpdateFileStructureDto input)
    {
        return _fileStructureAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    public virtual Task<FileStructureDto> UpdateAsync(Guid id, CreateUpdateFileStructureDto input)
    {
        return _fileStructureAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _fileStructureAppService.DeleteAsync(id);
    }

    [HttpGet]
    [Route("defaults/{key}")]
    public virtual Task<FileStructureDefaultDto?> GetDefaultConfigAsync(string key)
    {
        return _fileStructureAppService.GetDefaultConfigAsync(key);
    }

    [HttpGet]
    [Route("defaults")]
    public virtual Task<List<FileStructureDefaultDto>> GetAllDefaultConfigsAsync()
    {
        return _fileStructureAppService.GetAllDefaultConfigsAsync();
    }

    [HttpPost]
    [Route("{id}/reset-to-default")]
    public virtual Task<FileStructureDto> ResetToDefaultAsync(Guid id)
    {
        return _fileStructureAppService.ResetToDefaultAsync(id);
    }

    [HttpGet]
    [Route("{id}/is-modified")]
    public virtual Task<bool> IsModifiedFromDefaultAsync(Guid id)
    {
        return _fileStructureAppService.IsModifiedFromDefaultAsync(id);
    }


}
