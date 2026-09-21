using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.Permissions;
using Volo.Abp;

namespace SufiChain.SufiPlatform.FileManager.FileMigration;

[Authorize(FileManagerPermissions.StorageSettings.Manage)]
public class FileMigrationAppService : SufiApplicationService, IFileMigrationAppService
{
    private readonly IFileItemRepository _fileItemRepository;
    private readonly FileBlobMigrationService _blobMigrationService;

    public FileMigrationAppService(
        IFileItemRepository fileItemRepository,
        FileBlobMigrationService blobMigrationService)
    {
        _fileItemRepository = fileItemRepository;
        _blobMigrationService = blobMigrationService;
    }

    public virtual async Task<FileMigrationResultDto> MigrateStructureAsync(StartStructureMigrationInput input)
    {
        if (string.IsNullOrWhiteSpace(input.StructureKey))
        {
            throw new UserFriendlyException("Structure key is required.");
        }

        var maxFiles = Math.Clamp(input.MaxFiles, 1, 1000);
        var files = await _fileItemRepository.GetByStructureKeyAsync(input.StructureKey);
        var result = new FileMigrationResultDto { StructureKey = input.StructureKey };

        foreach (var file in files.Where(f => !f.IsTemp).Take(maxFiles))
        {
            result.Examined++;
            try
            {
                var migrated = await _blobMigrationService.MigrateFileAsync(
                    file,
                    deleteSourceAfterVerify: input.DeleteSourceAfterVerify);
                if (migrated)
                {
                    result.Migrated++;
                }
                else
                {
                    result.Skipped++;
                }
            }
            catch (Exception)
            {
                result.Failed++;
            }
        }

        return result;
    }

    public virtual async Task<FileMigrationResultDto> MoveFileToStructureAsync(MoveFileToStructureInput input)
    {
        if (string.IsNullOrWhiteSpace(input.TargetStructureKey))
        {
            throw new UserFriendlyException("Target structure key is required.");
        }

        var file = await _fileItemRepository.GetAsync(input.FileId);
        var result = new FileMigrationResultDto
        {
            StructureKey = input.TargetStructureKey,
            Examined = 1
        };

        try
        {
            var migrated = await _blobMigrationService.MigrateFileAsync(
                file,
                input.TargetStructureKey,
                input.TargetFolderId,
                input.DeleteSourceAfterVerify);
            if (migrated)
            {
                result.Migrated = 1;
            }
            else
            {
                result.Skipped = 1;
            }
        }
        catch (Exception ex)
        {
            result.Failed = 1;
            result.Error = ex.Message;
        }

        return result;
    }
}
