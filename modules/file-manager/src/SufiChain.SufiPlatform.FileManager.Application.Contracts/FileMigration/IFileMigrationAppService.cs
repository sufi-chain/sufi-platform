using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.FileManager.FileMigration;

[RemoteService(Name = SufiChain.SufiPlatform.FileManager.FileManagerRemoteServiceConsts.RemoteServiceName)]
public interface IFileMigrationAppService : IApplicationService
{
    Task<FileMigrationResultDto> MigrateStructureAsync(StartStructureMigrationInput input);

    Task<FileMigrationResultDto> MoveFileToStructureAsync(MoveFileToStructureInput input);
}

public class StartStructureMigrationInput
{
    public string StructureKey { get; set; } = string.Empty;

    public bool DeleteSourceAfterVerify { get; set; } = true;

    public int MaxFiles { get; set; } = 200;
}

public class MoveFileToStructureInput
{
    public Guid FileId { get; set; }

    public string TargetStructureKey { get; set; } = string.Empty;

    public Guid? TargetFolderId { get; set; }

    public bool DeleteSourceAfterVerify { get; set; } = true;
}

public class FileMigrationResultDto
{
    public string StructureKey { get; set; } = string.Empty;

    public int Examined { get; set; }

    public int Migrated { get; set; }

    public int Skipped { get; set; }

    public int Failed { get; set; }

    public string? Error { get; set; }
}
