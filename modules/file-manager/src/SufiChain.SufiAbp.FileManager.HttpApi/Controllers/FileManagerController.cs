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
[Route("api/sabp/file-manager/operations")]
public class FileManagerController : SufiAbpControllerBase, IFileManagerAppService
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
