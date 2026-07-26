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

    /// <summary>
    /// Chat FileManager file ids attached to the outbound message. Connectors resolve these
    /// into channel-specific media references (e.g. access tokens) before sending.
    /// </summary>
    public List<Guid> AttachmentFileIds { get; set; } = new();

    /// <summary>
    /// True when this is a cold first contact to the session (no prior operator-sent message
    /// exists for the thread). Multi-account channels (e.g. Telegram user) use this to apply
    /// per-connection first-contact daily caps. Computed by the dispatcher from connector
    /// metadata; defaults to false so single-account channels (Email) are unaffected.
    /// </summary>
    public bool IsFirstContact { get; set; }
}
