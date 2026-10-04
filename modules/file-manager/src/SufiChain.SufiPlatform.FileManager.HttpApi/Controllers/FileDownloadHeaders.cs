using System.Text;

namespace SufiChain.SufiPlatform.FileManager.Controllers;

/// <summary>
/// Content type and Content-Disposition values for file download responses.
/// The disposition value is ASCII so a Persian file name can be sent under RFC 5987.
/// </summary>
public static class FileDownloadHeaders
{
    public const string OctetStream = "application/octet-stream";

    public static string NormalizeContentType(string? mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
        {
            return OctetStream;
        }

        var trimmed = mimeType.Trim();
        foreach (var ch in trimmed)
        {
            if (ch < ' ' || ch > '~')
            {
                return OctetStream;
            }
        }

        return trimmed;
    }

    public static string Attachment(string fileName)
    {
        var safeName = SanitizeFileName(fileName);
        var fallback = ToAsciiFallback(safeName);
        var encoded = EncodeRfc5987(safeName);
        return $"attachment; filename=\"{fallback}\"; filename*=UTF-8''{encoded}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = fileName.Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal)
            .Trim();
        name = Path.GetFileName(name);
        if (string.IsNullOrWhiteSpace(name) || name is "." or "..")
        {
            return "download";
        }

        return name;
    }

    private static string ToAsciiFallback(string fileName)
    {
        var builder = new StringBuilder(fileName.Length);
        foreach (var ch in fileName)
        {
            if (ch is >= ' ' and <= '~' and not '"' and not '\\')
            {
                builder.Append(ch);
            }
            else
            {
                builder.Append('_');
            }
        }

        var fallback = builder.ToString().Trim();
        return fallback.Length == 0 ? "download" : fallback;
    }

    private static string EncodeRfc5987(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var builder = new StringBuilder(bytes.Length * 3);
        foreach (var b in bytes)
        {
            if (IsAttrChar(b))
            {
                builder.Append((char)b);
            }
            else
            {
                builder.Append('%');
                builder.Append(b.ToString("X2"));
            }
        }

        return builder.ToString();
    }

    private static bool IsAttrChar(byte b) =>
        b is >= (byte)'a' and <= (byte)'z'
            or >= (byte)'A' and <= (byte)'Z'
            or >= (byte)'0' and <= (byte)'9'
            or (byte)'!' or (byte)'#' or (byte)'$' or (byte)'&' or (byte)'+' or (byte)'-'
            or (byte)'.' or (byte)'^' or (byte)'_' or (byte)'`' or (byte)'|' or (byte)'~';
}
