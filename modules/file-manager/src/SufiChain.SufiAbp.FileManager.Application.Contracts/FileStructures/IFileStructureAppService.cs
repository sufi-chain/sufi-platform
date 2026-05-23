using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.SufiAbp.FileManager.FileStructures;

[RemoteService(Name = FileManagerRemoteServiceConsts.RemoteServiceName)]
public interface IFileStructureAppService : ISufiAbpCrudAppService<
    FileStructureDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateFileStructureDto,
    CreateUpdateFileStructureDto>
{
    /// <summary>
    /// Get file structure by key
    /// </summary>
    Task<FileStructureDto> GetByKeyAsync(string key);

    /// <summary>
    /// Check if a file structure with the given key exists
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Get the default (developer-defined) configuration for a structure key
    /// </summary>
    Task<FileStructureDefaultDto?> GetDefaultConfigAsync(string key);

    /// <summary>
    /// Get all developer-defined default structure configurations
    /// </summary>
    Task<List<FileStructureDefaultDto>> GetAllDefaultConfigsAsync();

    /// <summary>
    /// Reset a structure to its default (developer-defined) configuration
    /// </summary>
    Task<FileStructureDto> ResetToDefaultAsync(Guid id);

    /// <summary>
    /// Check if a structure has been modified from its default configuration
    /// </summary>
    Task<bool> IsModifiedFromDefaultAsync(Guid id);
    Task<PagedResultDto<FileStructureDto>> GetListAsync(PagedAndSortedResultRequestDto input);
}
