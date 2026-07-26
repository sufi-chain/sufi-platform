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

    /// <summary>
    /// When the dispatch was rate-limited (e.g. Telegram FloodWait / per-connection throttle),
    /// the suggested retry-after in seconds. Null when not applicable.
    /// </summary>
    public int? RetryAfterSeconds { get; set; }
}
