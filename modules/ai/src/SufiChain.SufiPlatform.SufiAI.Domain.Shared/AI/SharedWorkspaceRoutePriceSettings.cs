namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Host-only fallback prices for the shared workspace (sufi-chain#567).
/// A positive catalog quote or a positive price already stored on the route wins.
/// These values are USD amounts in the route's unit. They are not API keys.
/// Setting a value to 0 disables that floor.
/// </summary>
public static class SharedWorkspaceRoutePriceSettings
{
    public const string Prefix = "SufiAI.SharedWorkspace.RoutePrice.";

    public const string ChatInputPerMillionTokens = Prefix + "Chat.InputPerMillionTokens";
    public const string ChatOutputPerMillionTokens = Prefix + "Chat.OutputPerMillionTokens";
    public const string EmbeddingsInputPerMillionTokens = Prefix + "Embeddings.InputPerMillionTokens";
    public const string AudioPerMinute = Prefix + "Audio.PerMinute";
    public const string SpeechPerMillionCharacters = Prefix + "Speech.PerMillionCharacters";
    public const string VisionInputPerMillionTokens = Prefix + "Vision.InputPerMillionTokens";
    public const string VisionOutputPerMillionTokens = Prefix + "Vision.OutputPerMillionTokens";
    public const string ImagePerImage = Prefix + "Image.PerImage";
    public const string WebPerRequest = Prefix + "Web.PerRequest";
    public const string DecisionsInputPerMillionTokens = Prefix + "Decisions.InputPerMillionTokens";
    public const string DecisionsOutputPerMillionTokens = Prefix + "Decisions.OutputPerMillionTokens";

    public const string DefaultChatInputPerMillionTokens = "0.15";
    public const string DefaultChatOutputPerMillionTokens = "0.60";
    public const string DefaultEmbeddingsInputPerMillionTokens = "0.02";
    public const string DefaultAudioPerMinute = "0.006";
    public const string DefaultSpeechPerMillionCharacters = "15";
    public const string DefaultVisionInputPerMillionTokens = "0.15";
    public const string DefaultVisionOutputPerMillionTokens = "0.60";
    public const string DefaultImagePerImage = "0.04";
    public const string DefaultWebPerRequest = "0.01";
    public const string DefaultDecisionsInputPerMillionTokens = "0.15";
    public const string DefaultDecisionsOutputPerMillionTokens = "0.60";

    public static readonly (string Name, string DefaultValue)[] All =
    {
        (ChatInputPerMillionTokens, DefaultChatInputPerMillionTokens),
        (ChatOutputPerMillionTokens, DefaultChatOutputPerMillionTokens),
        (EmbeddingsInputPerMillionTokens, DefaultEmbeddingsInputPerMillionTokens),
        (AudioPerMinute, DefaultAudioPerMinute),
        (SpeechPerMillionCharacters, DefaultSpeechPerMillionCharacters),
        (VisionInputPerMillionTokens, DefaultVisionInputPerMillionTokens),
        (VisionOutputPerMillionTokens, DefaultVisionOutputPerMillionTokens),
        (ImagePerImage, DefaultImagePerImage),
        (WebPerRequest, DefaultWebPerRequest),
        (DecisionsInputPerMillionTokens, DefaultDecisionsInputPerMillionTokens),
        (DecisionsOutputPerMillionTokens, DefaultDecisionsOutputPerMillionTokens)
    };

    public static string? DefaultValue(string name)
    {
        foreach (var (settingName, value) in All)
        {
            if (settingName == name)
            {
                return value;
            }
        }

        return null;
    }
}
