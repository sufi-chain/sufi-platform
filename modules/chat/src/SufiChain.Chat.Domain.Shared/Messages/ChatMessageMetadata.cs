using System.Text.Json;
using System.Text.Json.Serialization;

namespace SufiChain.Chat.Messages;

public enum ChatMessageContentKind
{
    Text,
    Location,
    Voice,
    Mixed
}

public class ChatMessageLocationMetadata
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double? AccuracyMeters { get; set; }

    public string? Label { get; set; }
}

public class ChatMessageVoiceMetadata
{
    public int DurationSeconds { get; set; }

    public string MimeType { get; set; } = "audio/webm";
}

public class ChatMessageMetadataModel
{
    [JsonPropertyName("contentKind")]
    public ChatMessageContentKind ContentKind { get; set; } = ChatMessageContentKind.Text;

    [JsonPropertyName("location")]
    public ChatMessageLocationMetadata? Location { get; set; }

    [JsonPropertyName("voice")]
    public ChatMessageVoiceMetadata? Voice { get; set; }
}

public static class ChatMessageMetadata
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string BuildLocationJson(
        double latitude,
        double longitude,
        double? accuracyMeters = null,
        string? label = null)
    {
        return JsonSerializer.Serialize(new ChatMessageMetadataModel
        {
            ContentKind = ChatMessageContentKind.Location,
            Location = new ChatMessageLocationMetadata
            {
                Latitude = latitude,
                Longitude = longitude,
                AccuracyMeters = accuracyMeters,
                Label = label
            }
        }, SerializerOptions);
    }

    public static string BuildVoiceJson(int durationSeconds, string mimeType)
    {
        return JsonSerializer.Serialize(new ChatMessageMetadataModel
        {
            ContentKind = ChatMessageContentKind.Voice,
            Voice = new ChatMessageVoiceMetadata
            {
                DurationSeconds = durationSeconds,
                MimeType = mimeType
            }
        }, SerializerOptions);
    }

    public static string BuildMixedJson(ChatMessageMetadataModel model)
    {
        model.ContentKind = ChatMessageContentKind.Mixed;
        return JsonSerializer.Serialize(model, SerializerOptions);
    }

    public static ChatMessageMetadataModel? TryParse(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ChatMessageMetadataModel>(metadataJson, SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static string? GetOpenStreetMapUrl(ChatMessageLocationMetadata? location)
    {
        if (location == null)
        {
            return null;
        }

        return $"https://www.openstreetmap.org/?mlat={location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&mlon={location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}#map=16/{location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}/{location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
    }
}
