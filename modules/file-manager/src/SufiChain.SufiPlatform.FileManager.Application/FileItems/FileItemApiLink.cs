using System;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Builds the in-app file API path. An absolute <c>FileManager:BaseUrl</c> is a console
/// root and is ignored, so a tenant browser stays on its own host.
/// </summary>
public static class FileItemApiLink
{
    public static string Build(string? configuredBaseUrl, string relativePath)
    {
        var baseUrl = ResolveBase(configuredBaseUrl);
        var path = relativePath.StartsWith('/') ? relativePath : "/" + relativePath;
        if (baseUrl == "/")
        {
            return path;
        }

        return baseUrl.TrimEnd('/') + path;
    }

    public static string ResolveBase(string? configured)
    {
        if (string.IsNullOrWhiteSpace(configured))
        {
            return "/";
        }

        var trimmed = configured.Trim();
        if (trimmed == "/" || HasScheme(trimmed))
        {
            return "/";
        }

        return trimmed;
    }

    private static bool HasScheme(string value)
    {
        var schemeEnd = value.IndexOf(':');
        if (schemeEnd <= 0)
        {
            return false;
        }

        for (var i = 0; i < schemeEnd; i++)
        {
            if (!char.IsLetter(value[i]))
            {
                return false;
            }
        }

        return true;
    }
}
