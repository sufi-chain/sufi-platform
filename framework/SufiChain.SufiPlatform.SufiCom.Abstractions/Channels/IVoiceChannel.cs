namespace SufiChain.SufiPlatform.SufiCom.Channels;

/// <summary>
/// Outbound voice call channel (TTS - Text-to-Speech). Implement in separate NuGet packages.
/// </summary>
public interface IVoiceChannel : IChannel
{
    string ProviderCode { get; }

    int Priority { get; }

    void Configure(Dictionary<string, string> settings);

    Task<VoiceCallDeliveryResult> SendAsync(VoiceCallMessage message);

    Task<MessageDeliveryStatus> GetDeliveryStatusAsync(string externalId);

    Task<ProviderHealthCheckResult> HealthCheckAsync();

    Task<List<string>> GetSupportedFeaturesAsync();

    Task<SmsProviderMetadata> GetMetadataAsync();

    Task<List<ProviderSettingField>> GetRequiredSettingsAsync();
}