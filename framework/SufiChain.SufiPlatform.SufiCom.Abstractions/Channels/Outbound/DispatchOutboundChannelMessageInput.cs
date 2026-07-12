using System.ComponentModel.DataAnnotations;
using SufiChain.SufiPlatform.SufiCom.Channels.Metadata;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Outbound;

/// <summary>
/// Outbound message dispatch request from Chat to a channel connector.
/// </summary>
public class DispatchOutboundChannelMessageInput
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid MessageId { get; set; }

    [Required]
    [StringLength(ChannelConsts.MaxMessageBodyLength)]
    public string Body { get; set; } = string.Empty;

    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string? ExternalThreadId { get; set; }

    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string? ReplyToExternalMessageId { get; set; }

    public Guid? OperatorUserId { get; set; }

    public ChannelMessageConnectorMetadata? MessageConnectorMetadata { get; set; }

    public ChannelSessionConnectorMetadata SessionConnectorMetadata { get; set; } = new();
}
