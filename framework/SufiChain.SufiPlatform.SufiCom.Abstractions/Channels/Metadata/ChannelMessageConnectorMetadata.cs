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

    /// <summary>
    /// Connection identifier for multi-account channels (e.g. Telegram user phones).
    /// Null for single-account channels such as Email.
    /// </summary>
    public Guid? ConnectionId { get; set; }
}
