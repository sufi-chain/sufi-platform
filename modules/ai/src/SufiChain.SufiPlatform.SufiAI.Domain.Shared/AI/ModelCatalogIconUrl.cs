using System;
using System.Text.Json;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Accepts a catalog icon only when the payload already contains an https URL.
/// </summary>
public static class ModelCatalogIconUrl
{
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
        {
            return null;
        }

        return string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            ? uri.AbsoluteUri
            : null;
    }

    public static string? Read(JsonElement item)
    {
        return First(
            ReadString(item, "icon"),
            ReadString(item, "icon_url"),
            ReadString(item, "logo"),
            ReadString(item, "logo_url"));
    }

    public static string? First(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var normalized = Normalize(candidate);
            if (normalized != null)
            {
                return normalized;
            }
        }

        return null;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return property.GetString();
    }
}
