namespace SufiChain.SufiAbp.Communications.Channels;

/// <summary>
/// Default channel provider behaviors that cannot live on interfaces when targeting netstandard2.0.
/// </summary>
public static class ChannelProviderExtensions
{
    /// <summary>
    /// Returns provider capabilities; defaults to <see cref="ISmsChannel.GetSupportedFeaturesAsync"/>.
    /// </summary>
    public static Task<List<string>> GetCapabilitiesAsync(this ISmsChannel channel)
    {
        return channel.GetSupportedFeaturesAsync();
    }

    /// <summary>
    /// Returns provider capabilities; defaults to <see cref="IVoiceChannel.GetSupportedFeaturesAsync"/>.
    /// </summary>
    public static Task<List<string>> GetCapabilitiesAsync(this IVoiceChannel channel)
    {
        return channel.GetSupportedFeaturesAsync();
    }
}
