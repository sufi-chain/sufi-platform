using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.FileManager.FileFolders;
using Volo.Abp;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.FileManager.Controllers;

[Area(FileManagerRemoteServiceConsts.ModuleName)]
[RemoteService(Name = FileManagerRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/file-manager/folders")]
public class FolderController : SufiAbpControllerBase, IFolderAppService
{
    private readonly IFolderAppService _folderAppService;

    public FolderController(IFolderAppService folderAppService)
    {
        _folderAppService = folderAppService;
    }

    #region Tree Operations

    [HttpGet]
    [Route("tree")]
    public virtual Task<List<FolderTreeNodeDto>> GetTreeAsync(Guid? tenantId = null)
    {
        return _folderAppService.GetTreeAsync(tenantId);
    }

    [HttpGet]
    [Route("children")]
    public virtual Task<List<FolderTreeNodeDto>> GetChildrenAsync([FromQuery] Guid? parentId, [FromQuery] string? parentPath = null)
    {
        return _folderAppService.GetChildrenAsync(parentId, parentPath);
    }

    [HttpGet]
    [Route("contents")]
    public virtual Task<FolderContentsDto> GetContentsAsync([FromQuery] GetFolderContentsInput input)
    {
        return _folderAppService.GetContentsAsync(input);
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<FileFolderDto> GetAsync(Guid id)
    {
        return _folderAppService.GetAsync(id);
    }

    [HttpGet]
    [Route("by-path")]
    public virtual Task<FileFolderDto?> GetByPathAsync([FromQuery] string path)
    {
        return _folderAppService.GetByPathAsync(path);
    }

    [HttpPost]
    [Route("get-or-create-by-path")]
    public virtual Task<FileFolderDto?> GetOrCreateFolderByPathAsync([FromQuery] string path)
    {
        return _folderAppService.GetOrCreateFolderByPathAsync(path);
    }

    #endregion

    #region CRUD Operations

    [HttpPost]
    public virtual Task<FileFolderDto> CreateAsync(CreateFolderInput input)
    {
        return _folderAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}/rename")]
    public virtual Task<FileFolderDto> RenameAsync(Guid id, RenameFolderInput input)
    {
        return _folderAppService.RenameAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id, [FromQuery] bool recursive = false)
    {
        return _folderAppService.DeleteAsync(id, recursive);
    }

    [HttpPost]
    [Route("{id}/move")]
    public virtual Task<FileFolderDto> MoveAsync(Guid id, MoveFolderInput input)
    {
        return _folderAppService.MoveAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/copy")]
    public virtual Task<FileFolderDto> CopyAsync(Guid id, [FromQuery] Guid? targetParentId)
    {
        return _folderAppService.CopyAsync(id, targetParentId);
    }

    #endregion

    #region Permissions

    [HttpPut]
    [Route("{folderId}/permissions")]
    public virtual Task SetPermissionsAsync(Guid folderId, SetFolderPermissionsInput input)
    {
        return _folderAppService.SetPermissionsAsync(folderId, input);
    }

    [HttpGet]
    [Route("{folderId}/permissions")]
    public virtual Task<List<FolderPermissionDto>> GetPermissionsAsync(Guid folderId)
    {
        return _folderAppService.GetPermissionsAsync(folderId);
    }

    [HttpGet]
    [Route("{folderId}/has-permission")]
    public virtual Task<bool> HasPermissionAsync(Guid folderId, [FromQuery] FolderPermissionLevelDto level)
    {
        return _folderAppService.HasPermissionAsync(folderId, level);
    }

    #endregion

    #region Sharing

    [HttpPost]
    [Route("{folderId}/share")]
    public virtual Task ShareAsync(Guid folderId, ShareFolderInput input)
    {
        return _folderAppService.ShareAsync(folderId, input);
    }

    [HttpDelete]
    [Route("{folderId}/share/{tenantId}")]
    public virtual Task UnshareAsync(Guid folderId, Guid tenantId)
    {
        return _folderAppService.UnshareAsync(folderId, tenantId);
    }

    [HttpGet]
    [Route("shared")]
    public virtual Task<List<FolderTreeNodeDto>> GetSharedFoldersAsync()
    {
        return _folderAppService.GetSharedFoldersAsync();
    }

    #endregion

    #region Statistics

    [HttpGet]
    [Route("statistics")]
    public virtual Task<FolderStatisticsDto> GetStatisticsAsync([FromQuery] Guid? folderId, [FromQuery] string? path = null)
    {
        return _folderAppService.GetStatisticsAsync(folderId, path);
    }

    #endregion
}
