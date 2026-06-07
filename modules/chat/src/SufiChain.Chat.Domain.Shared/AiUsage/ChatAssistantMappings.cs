using System.Text.Json;
using System.Text.RegularExpressions;

namespace SufiChain.Chat.AiUsage;

/// <summary>
/// Serializes and queries tenant assistant mappings stored in settings JSON.
/// </summary>
public static partial class ChatAssistantMappings
{
    public const string EmptyJson = "[]";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

#if NETSTANDARD2_1
    private static readonly Regex KeyPattern = new(
        "^[a-z][a-z0-9_-]{0,31}$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);
#else
    [GeneratedRegex("^[a-z][a-z0-9_-]{0,31}$", RegexOptions.CultureInvariant)]
    private static partial Regex KeyPattern();
#endif

    public static List<ChatAssistantMappingItem> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<ChatAssistantMappingItem>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<ChatAssistantMappingItem>>(json, SerializerOptions)
                   ?? new List<ChatAssistantMappingItem>();
        }
        catch (JsonException)
        {
            return new List<ChatAssistantMappingItem>();
        }
    }

    public static string Serialize(IReadOnlyList<ChatAssistantMappingItem> mappings)
    {
        return JsonSerializer.Serialize(mappings ?? Array.Empty<ChatAssistantMappingItem>(), SerializerOptions);
    }

    public static bool IsMessengerVisible(ChatAssistantMappingItem item)
    {
        return item.IsEnabled && item.IsPublic is not false;
    }

    public static bool IsValidKey(string? key)
    {
#if NETSTANDARD2_1
        return !string.IsNullOrWhiteSpace(key) && KeyPattern.IsMatch(key.Trim());
#else
        return !string.IsNullOrWhiteSpace(key) && KeyPattern().IsMatch(key.Trim());
#endif
    }

    public static string? TryResolveWorkspaceName(
        IReadOnlyList<ChatAssistantMappingItem> mappings,
        string? assistantKey)
    {
        if (string.IsNullOrWhiteSpace(assistantKey))
        {
            return null;
        }

        var normalizedKey = assistantKey.Trim();
        var match = mappings.FirstOrDefault(item =>
            item.IsEnabled &&
            item.Key.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase));

        return string.IsNullOrWhiteSpace(match?.WorkspaceName) ? null : match.WorkspaceName.Trim();
    }

    public static IReadOnlyList<ChatAssistantMappingItem> Normalize(IReadOnlyList<ChatAssistantMappingItem> mappings)
    {
        return mappings
            .Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .Select(item => new ChatAssistantMappingItem
            {
                Key = item.Key.Trim().ToLowerInvariant(),
                DisplayName = item.DisplayName?.Trim() ?? string.Empty,
                WorkspaceName = item.WorkspaceName?.Trim() ?? string.Empty,
                IsEnabled = item.IsEnabled,
                IsPublic = item.IsPublic ?? true
            })
            .ToList();
    }
}
