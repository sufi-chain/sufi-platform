using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.Communications.Channels.Metadata;

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
}
