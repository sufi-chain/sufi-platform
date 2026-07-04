namespace SufiChain.SufiAbp.Communications.Channels;

/// <summary>
/// Outbound SMS channel. Implement in separate NuGet packages; auto-discovered via ABP DI
/// when implementing ITransientDependency.
/// </summary>
public interface ISmsChannel : IChannel
{
    string ProviderCode { get; }

    int Priority => 0;

    void Configure(Dictionary<string, string> settings);

    Task<SmsDeliveryResult> SendAsync(SmsMessage message);

    Task<MessageDeliveryStatus> GetDeliveryStatusAsync(string externalId);

    Task<ProviderHealthCheckResult> HealthCheckAsync();

    Task<List<string>> GetSupportedFeaturesAsync();

    async Task<List<string>> GetCapabilitiesAsync()
    {
        return await GetSupportedFeaturesAsync();
    }

    Task<SmsProviderMetadata> GetMetadataAsync();

    Task<List<ProviderSettingField>> GetRequiredSettingsAsync();
}