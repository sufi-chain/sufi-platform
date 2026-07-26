using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Metadata;

/// <summary>
/// Connector-specific session metadata persisted as namespaced extra properties.
/// </summary>
public class ChannelSessionConnectorMetadata
{
    [Required]
    [StringLength(ChannelConsts.MaxConnectorNameLength)]
    public string ConnectorName { get; set; } = string.Empty;

    [Required]
    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string ExternalThreadId { get; set; } = string.Empty;

    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string? LastExternalMessageId { get; set; }

    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string? InReplyToExternalMessageId { get; set; }

    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string? ExternalParticipantAddress { get; set; }

    [StringLength(ChannelConsts.MaxDisplayNameLength)]
    public string? ExternalParticipantName { get; set; }

    /// <summary>
    /// Connection identifier for multi-account channels (e.g. Telegram user phones).
    /// Null for single-account channels such as Email.
    /// </summary>
    public Guid? ConnectionId { get; set; }
}
