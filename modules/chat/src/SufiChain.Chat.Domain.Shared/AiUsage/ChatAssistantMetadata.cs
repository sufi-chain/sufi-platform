using System.Text.Json;

namespace SufiChain.Chat.AiUsage;

/// <summary>
/// Metadata keys and helpers for assistant chat sessions.
/// </summary>
public static class ChatAssistantMetadata
{
    public const string WorkspaceNameKey = "aiWorkspaceName";

    public const string AssistantKeyKey = "assistantKey";

    public const string OrchestrationModeKey = "orchestrationMode";

    public const string ExternalOrchestrationMode = "external";

    public static string BuildJson(string workspaceName, string? assistantKey = null, bool externalOrchestration = false)
    {
        var metadata = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(workspaceName))
        {
            metadata[WorkspaceNameKey] = workspaceName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(assistantKey))
        {
            metadata[AssistantKeyKey] = assistantKey.Trim().ToLowerInvariant();
        }

        if (externalOrchestration)
        {
            metadata[OrchestrationModeKey] = ExternalOrchestrationMode;
        }

        return metadata.Count == 0
            ? string.Empty
            : JsonSerializer.Serialize(metadata);
    }

    public static string? TryGetWorkspaceName(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(metadataJson);
            if (document.RootElement.TryGetProperty(WorkspaceNameKey, out var property) &&
                property.ValueKind == JsonValueKind.String)
            {
                var value = property.GetString();
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    public static bool IsExternallyOrchestrated(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(metadataJson);
            if (document.RootElement.TryGetProperty(OrchestrationModeKey, out var property) &&
                property.ValueKind == JsonValueKind.String)
            {
                return string.Equals(property.GetString(), ExternalOrchestrationMode, StringComparison.OrdinalIgnoreCase);
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }

    public static string? TryGetAssistantKey(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(metadataJson);
            if (document.RootElement.TryGetProperty(AssistantKeyKey, out var property) &&
                property.ValueKind == JsonValueKind.String)
            {
                var value = property.GetString();
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}

/// <summary>
/// Well-known demo and integration workspace names for multi-assistant setups.
/// </summary>
public static class ChatAssistantWorkspaceNames
{
    public const string Support = "support";

    public const string Sales = "sales";
}
