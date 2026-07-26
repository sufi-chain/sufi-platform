using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using Volo.Abp;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiPlatform.FileManager.Controllers;

[Area(FileManagerRemoteServiceConsts.ModuleName)]
[RemoteService(Name = FileManagerRemoteServiceConsts.RemoteServiceName)]
[Route("api/file-manager/operations")]
public class FileManagerController : SufiControllerBase, IFileManagerAppService
{
    private readonly IFileManagerAppService _fileManagerAppService;

    public FileManagerController(IFileManagerAppService fileManagerAppService)
    {
        _fileManagerAppService = fileManagerAppService;
    }

    #region Clipboard Operations

    [HttpPost]
    [Route("clipboard/cut")]
    public virtual Task<ClipboardResultDto> CutAsync(ClipboardOperationInput input)
    {
        return _fileManagerAppService.CutAsync(input);
    }

    [HttpPost]
    [Route("clipboard/copy")]
    public virtual Task<ClipboardResultDto> CopyAsync(ClipboardOperationInput input)
    {
        return _fileManagerAppService.CopyAsync(input);
    }

    [HttpPost]
    [Route("clipboard/paste")]
    public virtual Task<PasteResultDto> PasteAsync(PasteInput input)
    {
        return _fileManagerAppService.PasteAsync(input);
    }

    [HttpGet]
    [Route("clipboard")]
    public virtual Task<ClipboardStateDto> GetClipboardStateAsync()
    {
        return _fileManagerAppService.GetClipboardStateAsync();
    }

    [HttpDelete]
    [Route("clipboard")]
    public virtual Task ClearClipboardAsync()
    {
        return _fileManagerAppService.ClearClipboardAsync();
    }

    #endregion

    #region Bulk Operations

    [HttpPost]
    [Route("bulk/move")]
    public virtual Task<BulkOperationResultDto> MoveItemsAsync(BulkMoveInput input)
    {
        return _fileManagerAppService.MoveItemsAsync(input);
    }

    [HttpPost]
    [Route("bulk/copy")]
    public virtual Task<BulkOperationResultDto> CopyItemsAsync(BulkCopyInput input)
    {
        return _fileManagerAppService.CopyItemsAsync(input);
    }

    [HttpDelete]
    [Route("bulk/delete")]
    public virtual Task<BulkOperationResultDto> DeleteItemsAsync([FromBody] BulkDeleteInput input)
    {
        return _fileManagerAppService.DeleteItemsAsync(input);
    }

    [HttpPost]
    [Route("bulk/download")]
    public virtual Task<DownloadResultDto> DownloadAsZipAsync(DownloadInput input)
    {
        return _fileManagerAppService.DownloadAsZipAsync(input);
    }

    [HttpGet]
    [Route("zip-downloads/{token}")]
    public virtual async Task<IActionResult> DownloadZipFileAsync(string token)
    {
        var content = await _fileManagerAppService.GetZipDownloadAsync(token);
        if (content == null)
        {
            return NotFound();
        }

        return File(content.Content, content.ContentType, content.FileName);
    }

    Task<ZipDownloadContentDto?> IFileManagerAppService.GetZipDownloadAsync(string token)
    {
        return _fileManagerAppService.GetZipDownloadAsync(token);
    }

    #endregion

    #region Search

    [HttpPost]
    [Route("search")]
    public virtual Task<SearchResultDto> SearchAsync(SearchInput input)
    {
        return _fileManagerAppService.SearchAsync(input);
    }

    #endregion
}
