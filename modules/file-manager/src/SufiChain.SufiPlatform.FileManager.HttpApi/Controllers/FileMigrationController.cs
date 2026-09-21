using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.FileManager.FileMigration;
using Volo.Abp;

namespace SufiChain.SufiPlatform.FileManager.Controllers;

[Area(FileManagerRemoteServiceConsts.ModuleName)]
[RemoteService(Name = FileManagerRemoteServiceConsts.RemoteServiceName)]
[Route("api/file-manager/migrations")]
public class FileMigrationController : SufiControllerBase, IFileMigrationAppService
{
    private readonly IFileMigrationAppService _fileMigrationAppService;

    public FileMigrationController(IFileMigrationAppService fileMigrationAppService)
    {
        _fileMigrationAppService = fileMigrationAppService;
    }

    [HttpPost]
    [Route("structure")]
    public virtual Task<FileMigrationResultDto> MigrateStructureAsync(StartStructureMigrationInput input)
    {
        return _fileMigrationAppService.MigrateStructureAsync(input);
    }

    [HttpPost]
    [Route("file")]
    public virtual Task<FileMigrationResultDto> MoveFileToStructureAsync(MoveFileToStructureInput input)
    {
        return _fileMigrationAppService.MoveFileToStructureAsync(input);
    }
}
