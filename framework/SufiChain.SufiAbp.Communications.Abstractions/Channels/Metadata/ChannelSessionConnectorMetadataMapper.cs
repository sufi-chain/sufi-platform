using System.Text.Json;
using Volo.Abp;

namespace SufiChain.SufiAbp.Communications.Channels.Metadata;

/// <summary>
/// Maps connector external identifiers to and from MetadataJson payloads.
/// </summary>
public static class ChannelSessionConnectorMetadataMapper
{
    public const string ConnectorPropertyName = "connector";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string BuildSessionMetadata(
        ChannelSessionConnectorMetadata connectorMetadata,
        string? existingMetadataJson = null)
    {
        Check.NotNull(connectorMetadata, nameof(connectorMetadata));
        Check.NotNullOrWhiteSpace(connectorMetadata.ConnectorName, nameof(connectorMetadata.ConnectorName));
        Check.NotNullOrWhiteSpace(connectorMetadata.ExternalThreadId, nameof(connectorMetadata.ExternalThreadId));

        var root = ParseRoot(existingMetadataJson);
        root[ConnectorPropertyName] = JsonSerializer.SerializeToElement(connectorMetadata, SerializerOptions);
        return JsonSerializer.Serialize(root, SerializerOptions);
    }

    public static string BuildMessageMetadata(ChannelMessageConnectorMetadata connectorMetadata)
    {
        Check.NotNull(connectorMetadata, nameof(connectorMetadata));
        Check.NotNullOrWhiteSpace(connectorMetadata.ConnectorName, nameof(connectorMetadata.ConnectorName));
        Check.NotNullOrWhiteSpace(connectorMetadata.ExternalMessageId, nameof(connectorMetadata.ExternalMessageId));

        var root = new Dictionary<string, JsonElement>
        {
            [ConnectorPropertyName] = JsonSerializer.SerializeToElement(connectorMetadata, SerializerOptions)
        };

        return JsonSerializer.Serialize(root, SerializerOptions);
    }

    public static ChannelSessionConnectorMetadata? TryReadSessionMetadata(string? metadataJson)
    {
        if (metadataJson.IsNullOrWhiteSpace())
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(metadataJson);
            if (!document.RootElement.TryGetProperty(ConnectorPropertyName, out var connectorElement))
            {
                return null;
            }

            return connectorElement.Deserialize<ChannelSessionConnectorMetadata>(SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static ChannelMessageConnectorMetadata? TryReadMessageMetadata(string? metadataJson)
    {
        if (metadataJson.IsNullOrWhiteSpace())
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(metadataJson);
            if (!document.RootElement.TryGetProperty(ConnectorPropertyName, out var connectorElement))
            {
                return null;
            }

            return connectorElement.Deserialize<ChannelMessageConnectorMetadata>(SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static string BuildLookupToken(string connectorName, string externalThreadId)
    {
        return "\"connectorName\":\"" + connectorName + "\",\"externalThreadId\":\"" + externalThreadId + "\"";
    }

    private static Dictionary<string, JsonElement> ParseRoot(string? existingMetadataJson)
    {
        if (existingMetadataJson.IsNullOrWhiteSpace())
        {
            return new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(existingMetadataJson, SerializerOptions)
                   ?? new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        }
        catch (JsonException)
        {
            return new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        }
    }
}