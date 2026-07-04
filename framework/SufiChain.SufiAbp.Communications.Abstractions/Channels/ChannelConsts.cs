namespace SufiChain.SufiAbp.Communications.Channels;

/// <summary>
/// Validation constants for channel connector DTOs.
/// </summary>
public static class ChannelConsts
{
    public const int MaxConnectorNameLength = 64;
    public const int MaxExternalIdLength = 512;
    public const int MaxMetadataJsonLength = 8192;
    public const int MaxMessageBodyLength = 16000;
    public const int MaxTitleLength = 256;
    public const int MaxDisplayNameLength = 128;
    public const int MaxAnonymousVisitorIdLength = 128;
}