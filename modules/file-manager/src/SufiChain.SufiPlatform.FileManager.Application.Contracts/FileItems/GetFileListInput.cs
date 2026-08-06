using System;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

public class GetFileListInput : PagedAndSortedResultRequestDto
{
    public string? Keyword { get; set; }
    public FileType? FileType { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? StructureKey { get; set; }
    public bool? OnlyFromPublicStructures { get; set; }
    public bool? IsTemp { get; set; }
}

public class UpdateFileMetadataInput
{
    public string? Name { get; set; }
    public string? Alt { get; set; }
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
}

public class StorageQuotaDto
{
    public long UsedBytes { get; set; }
    public double UsedMB { get; set; }
    public long LimitMB { get; set; }
    public double AvailableMB { get; set; }
    public double PercentageUsed { get; set; }
}
