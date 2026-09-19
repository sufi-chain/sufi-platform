using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Filename and folder-path checks for upload and folder create.
/// Rejects traversal, absolute paths, and a fixed executable/script deny list.
/// </summary>
public static class FilePathSecurity
{
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "exe", "bat", "cmd", "com", "scr", "pif", "msi", "dll",
        "ps1", "psm1", "vbs", "vbe", "js", "jse", "wsf", "wsh",
        "hta", "cpl", "msc",
        "php", "php3", "phtml", "aspx", "asp", "jsp", "jspx",
        "cgi", "pl", "py", "rb", "sh",
        "html", "htm", "shtml", "svg", "xml"
    };

    private static readonly HashSet<char> AlwaysInvalidNameChars = new()
    {
        '/', '\\', ':', '*', '?', '"', '<', '>', '|', '\0'
    };

    public static bool IsSafeUploadFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        if (fileName.IndexOf('\0') >= 0)
        {
            return false;
        }

        var normalized = fileName.Replace('\\', '/').Trim();
        if (normalized.Length == 0 || normalized.EndsWith("/", StringComparison.Ordinal))
        {
            return false;
        }

        if (Path.IsPathRooted(fileName) || Path.IsPathRooted(normalized))
        {
            return false;
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 1)
        {
            return false;
        }

        var leaf = segments[0];
        if (leaf is "." or ".." || leaf.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        if (leaf.IndexOfAny(AlwaysInvalidNameChars.ToArray()) >= 0)
        {
            return false;
        }

        return Path.GetFileName(leaf) == leaf;
    }

    public static bool IsBlockedExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return true;
        }

        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        return BlockedExtensions.Contains(extension);
    }

    public static string SanitizeFolderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var invalid = Path.GetInvalidFileNameChars()
            .Concat(AlwaysInvalidNameChars)
            .Distinct()
            .ToArray();

        var sanitized = new string(name.Where(c => !invalid.Contains(c)).ToArray())
            .Trim()
            .Replace(' ', '-')
            .ToLowerInvariant();

        if (sanitized is "." or ".." || sanitized.Length == 0)
        {
            return string.Empty;
        }

        if (sanitized.Contains("..", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        return sanitized;
    }
}
