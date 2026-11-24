namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Components.Upload;

/// <summary>
/// Matches a file against an HTML accept list (extensions, mime types, and type/*).
/// An empty accept list allows the file.
/// </summary>
public static class PublicUploadAccept
{
    public static bool IsAllowed(string? accept, string? fileName, string? contentType)
    {
        if (string.IsNullOrWhiteSpace(accept))
        {
            return true;
        }

        var tokens = accept.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
        {
            return true;
        }

        var extension = System.IO.Path.GetExtension(fileName ?? string.Empty);
        var type = contentType ?? string.Empty;
        foreach (var token in tokens)
        {
            if (token.StartsWith('.') && extension.Equals(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (token.EndsWith("/*", StringComparison.Ordinal) &&
                type.StartsWith(token[..^1], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (type.Equals(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
