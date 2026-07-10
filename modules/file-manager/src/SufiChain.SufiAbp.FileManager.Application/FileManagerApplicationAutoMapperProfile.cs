using System;
using System.Collections.Generic;
using System.Text.Json;
using Riok.Mapperly.Abstractions;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiAbp.FileManager;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[MapExtraProperties]
public partial class FileItemToFileItemDtoMapper : MapperBase<FileItem, FileItemDto>
{
    [MapperIgnoreTarget(nameof(FileItemDto.StructureIsPublicAccess))]
    [MapperIgnoreTarget(nameof(FileItemDto.StructureBaseUrl))]
    [MapperIgnoreTarget(nameof(FileItemDto.StructureStorageProvider))]
    public override partial FileItemDto Map(FileItem source);

    [MapperIgnoreTarget(nameof(FileItemDto.StructureIsPublicAccess))]
    [MapperIgnoreTarget(nameof(FileItemDto.StructureBaseUrl))]
    [MapperIgnoreTarget(nameof(FileItemDto.StructureStorageProvider))]
    public override partial void Map(FileItem source, FileItemDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[MapExtraProperties]
public partial class FileStructureToFileStructureDtoMapper : MapperBase<FileStructure, FileStructureDto>
{
    [MapperIgnoreTarget(nameof(FileStructureDto.HasDefaultConfig))]
    [MapperIgnoreTarget(nameof(FileStructureDto.IsModifiedFromDefault))]
    [MapperIgnoreTarget(nameof(FileStructureDto.IsStatic))]
    [MapperIgnoreTarget(nameof(FileStructureDto.StorageConfig))]
    public override partial FileStructureDto Map(FileStructure source);

    [MapperIgnoreTarget(nameof(FileStructureDto.HasDefaultConfig))]
    [MapperIgnoreTarget(nameof(FileStructureDto.IsModifiedFromDefault))]
    [MapperIgnoreTarget(nameof(FileStructureDto.IsStatic))]
    [MapperIgnoreTarget(nameof(FileStructureDto.StorageConfig))]
    public override partial void Map(FileStructure source, FileStructureDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class CreateUpdateFileStructureDtoToFileStructureMapper : MapperBase<CreateUpdateFileStructureDto, FileStructure>
{
    public override FileStructure Map(CreateUpdateFileStructureDto source)
    {
        var destination = new FileStructure(Guid.NewGuid(), source.Key, source.DisplayName,
            source.AllowedFileTypes, source.AllowedExtensions, source.AllowedMimeTypes, source.MaxFileSize);
        Map(source, destination);
        return destination;
    }

    [MapperIgnoreTarget(nameof(FileStructure.Id))]
    [MapperIgnoreTarget(nameof(FileStructure.ExtraProperties))]
    [MapperIgnoreTarget(nameof(FileStructure.CreationTime))]
    [MapperIgnoreTarget(nameof(FileStructure.CreatorId))]
    [MapperIgnoreTarget(nameof(FileStructure.LastModificationTime))]
    [MapperIgnoreTarget(nameof(FileStructure.LastModifierId))]
    [MapperIgnoreTarget(nameof(FileStructure.ConcurrencyStamp))]
    [MapperIgnoreSource(nameof(CreateUpdateFileStructureDto.StorageConfig))]
    public override partial void Map(CreateUpdateFileStructureDto source, FileStructure destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[MapExtraProperties]
public partial class FileFolderToFileFolderDtoMapper : MapperBase<FileFolder, FileFolderDto>
{
    [MapperIgnoreTarget(nameof(FileFolderDto.Type))]
    [MapperIgnoreTarget(nameof(FileFolderDto.SharedWithTenants))]
    public override partial FileFolderDto Map(FileFolder source);

    [MapperIgnoreTarget(nameof(FileFolderDto.Type))]
    [MapperIgnoreTarget(nameof(FileFolderDto.SharedWithTenants))]
    public override partial void Map(FileFolder source, FileFolderDto destination);

    public override void AfterMap(FileFolder source, FileFolderDto destination)
    {
        destination.Type = source.Type switch
        {
            FolderType.TenantRoot => FolderTypeDto.TenantRoot,
            FolderType.Structure => FolderTypeDto.Structure,
            FolderType.YearMonth => FolderTypeDto.YearMonth,
            FolderType.Custom => FolderTypeDto.Custom,
            _ => FolderTypeDto.Custom
        };

        destination.SharedWithTenants = ParseSharedTenants(source.SharedWithTenants);
    }

    private static List<Guid>? ParseSharedTenants(string? json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json);
        }
        catch
        {
            return null;
        }
    }
}
