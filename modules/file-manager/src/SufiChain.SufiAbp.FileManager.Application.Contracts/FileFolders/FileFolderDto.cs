using System;
using System.Collections.Generic;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.FileManager.FileFolders;

/// <summary>
/// DTO for FileFolder entity
/// </summary>
public class FileFolderDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string Path { get; set; } = default!;
    public Guid? ParentId { get; set; }
    public FolderTypeDto Type { get; set; }
    public string? StructureKey { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public bool IsShared { get; set; }
    public List<Guid>? SharedWithTenants { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO enum matching FolderType
/// </summary>
public enum FolderTypeDto
{
    TenantRoot = 0,
    Structure = 1,
    YearMonth = 2,
    Custom = 3
}
