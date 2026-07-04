namespace SufiChain.SufiAbp.Communications.Channels;

/// <summary>
/// Root marker for all communication channel implementations (bidirectional connectors,
/// outbound SMS channels, outbound voice channels, etc.).
/// </summary>
public interface IChannel
{
    /// <summary>
    /// Unique channel name (e.g. <see cref="ChannelNames.Email"/>, <see cref="ChannelNames.SmsKavenegar"/>).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Channel origin stamped on sessions and messages created through this channel.
    /// </summary>
    ChannelOrigin ChannelOrigin { get; }
}