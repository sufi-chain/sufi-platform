using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Metadata;

/// <summary>
/// Connector-specific message metadata persisted as namespaced extra properties.
/// </summary>
public class ChannelMessageConnectorMetadata
{
    [Required]
    [StringLength(ChannelConsts.MaxConnectorNameLength)]
    public string ConnectorName { get; set; } = string.Empty;

    [Required]
    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string ExternalMessageId { get; set; } = string.Empty;

    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string? InReplyToExternalMessageId { get; set; }
}
