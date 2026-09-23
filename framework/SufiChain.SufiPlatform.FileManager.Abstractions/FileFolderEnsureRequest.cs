using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.FileManager;

[Serializable]
public class FileFolderEnsureRequest
{
    [Required]
    public string StructureKey { get; set; } = string.Empty;

    /// <summary>Absolute folder path under the structure root, e.g. <c>/AI/Hooshvares/helpdesk.livechat</c>.</summary>
    [Required]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Stored leaf folder name. May be a business localization key resolved with
    /// <see cref="LocalizationResourceName"/> (or the structure resource when omitted).
    /// Defaults to the last path segment.
    /// </summary>
    public string? Name { get; set; }

    public string? LocalizationResourceName { get; set; }

    public string? Description { get; set; }
}

[Serializable]
public class FileFolderReferenceDto
{
    public Guid Id { get; set; }

    public string Path { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? StructureKey { get; set; }
}
