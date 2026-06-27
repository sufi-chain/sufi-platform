using System;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.FileManager.Blazor.Public.Editors;

public interface IFileGalleryDialogService
{
    bool IsHostRegistered { get; }
    Task<FileGalleryResult?> ShowImageGalleryAsync();
    Task<FileGalleryResult?> ShowFileGalleryAsync();
}

public class FileGalleryResult
{
    public Guid FileId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? Alt { get; set; }
    public string? MimeType { get; set; }
}
