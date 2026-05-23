using SufiChain.SufiAbp.FileManager.FileTypes;
using SufiChain.SufiBlazor;
using SufiChain.SufiBlazor.Components;

namespace SufiChain.SufiAbp.FileManager.Blazor.Helpers;

/// <summary>
/// Shared utility methods for FileManager components
/// </summary>
public static class FileManagerHelpers
{
    /// <summary>
    /// Formats a file size in bytes to a human-readable string (B, KB, MB, GB, TB)
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Gets the appropriate SbColor for a file type
    /// </summary>
    public static SbColor GetFileTypeColor(FileType fileType)
    {
        return fileType switch
        {
            FileType.Image => SbColor.Success,
            FileType.Video => SbColor.Info,
            FileType.Document => SbColor.Warning,
            FileType.Audio => SbColor.Primary,
            _ => SbColor.Default
        };
    }

    /// <summary>
    /// Gets the Sufi icon name for a file type (generic; use GetFileIconByMimeType for format-specific icons).
    /// </summary>
    public static string GetFileTypeIcon(FileType fileType)
    {
        return fileType switch
        {
            FileType.Image => "file-image",
            FileType.Video => "file-video",
            FileType.Document => "file-text",
            FileType.Audio => "file-audio",
            _ => "file"
        };
    }

    /// <summary>
    /// Gets the Sufi icon name for a file by MIME type (format-specific: file-pdf, file-doc, file-excel, etc.).
    /// </summary>
    public static string GetFileIconByMimeType(string? mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType)) return "file";
        var m = mimeType.ToLowerInvariant();
        if (m.StartsWith("image/")) return "file-image";
        if (m.StartsWith("video/")) return "file-video";
        if (m.StartsWith("audio/")) return "file-audio";
        if (m.Contains("pdf")) return "file-pdf";
        if (m.Contains("word") || m.Contains("msword") || m.Contains("opendocument.text")) return "file-doc";
        if (m.Contains("excel") || m.Contains("spreadsheet") || m.Contains("opendocument.spreadsheet")) return "file-excel";
        if (m.Contains("powerpoint") || m.Contains("presentation") || m.Contains("opendocument.presentation")) return "file-ppt";
        if (m.Contains("csv")) return "file-csv";
        if (m.Contains("json")) return "file-json";
        if (m.Contains("xml")) return "file-xml";
        if (m.Contains("text/plain")) return "file-text";
        if (m.Contains("zip") || m.Contains("archive") || m.Contains("x-rar") || m.Contains("x-7z")) return "file-archive";
        return "file";
    }

    /// <summary>
    /// Determines if a MIME type represents an image
    /// </summary>
    public static bool IsImageMimeType(string? mimeType)
    {
        return !string.IsNullOrEmpty(mimeType) && mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if a MIME type represents a video
    /// </summary>
    public static bool IsVideoMimeType(string? mimeType)
    {
        return !string.IsNullOrEmpty(mimeType) && mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if a MIME type represents audio
    /// </summary>
    public static bool IsAudioMimeType(string? mimeType)
    {
        return !string.IsNullOrEmpty(mimeType) && mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the file extension from a filename (without the dot)
    /// </summary>
    public static string GetFileExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return string.Empty;

        var lastDot = fileName.LastIndexOf('.');
        return lastDot >= 0 ? fileName[(lastDot + 1)..].ToLowerInvariant() : string.Empty;
    }

    /// <summary>
    /// Truncates a filename if it exceeds the maximum length
    /// </summary>
    public static string TruncateFileName(string fileName, int maxLength = 30)
    {
        if (string.IsNullOrEmpty(fileName) || fileName.Length <= maxLength)
            return fileName;

        var extension = GetFileExtension(fileName);
        var name = fileName;

        if (!string.IsNullOrEmpty(extension))
        {
            name = fileName[..^(extension.Length + 1)]; // Remove extension and dot
            var remainingLength = maxLength - extension.Length - 4; // -4 for "..." and "."
            
            if (remainingLength > 0)
            {
                return $"{name[..remainingLength]}...{extension}";
            }
        }

        return $"{fileName[..^3]}...";
    }
}
