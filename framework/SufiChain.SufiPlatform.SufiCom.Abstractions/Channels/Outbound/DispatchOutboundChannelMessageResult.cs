using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Outbound;

/// <summary>
/// Result of outbound connector dispatch.
/// </summary>
public class DispatchOutboundChannelMessageResult
{
    public bool Succeeded { get; set; }

    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string? ExternalMessageId { get; set; }

    public string? FailureReason { get; set; }
}