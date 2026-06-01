namespace SufiChain.Chat.Connectors.Email.Settings;

public class ChatEmailConnectorRuntimeSettings
{
    public bool Enabled { get; set; }

    public string? DefaultFromAddress { get; set; }

    public string? ReplyToAddress { get; set; }

    public ChatInboundEmailProtocol InboundProtocol { get; set; }

    public string? InboundHost { get; set; }

    public int InboundPort { get; set; }

    public bool InboundUseSsl { get; set; }

    public string? InboundUserName { get; set; }

    public string? InboundPassword { get; set; }

    public string? SmtpHost { get; set; }

    public int SmtpPort { get; set; }

    public bool SmtpUseSsl { get; set; }

    public string? SmtpUserName { get; set; }

    public string? SmtpPassword { get; set; }

    public bool IsOutboundConfigured =>
        Enabled &&
        !DefaultFromAddress.IsNullOrWhiteSpace() &&
        !SmtpHost.IsNullOrWhiteSpace() &&
        SmtpPort > 0;

    public bool IsInboundConfigured =>
        Enabled &&
        InboundProtocol != ChatInboundEmailProtocol.None &&
        !InboundHost.IsNullOrWhiteSpace() &&
        InboundPort > 0 &&
        !InboundUserName.IsNullOrWhiteSpace() &&
        !InboundPassword.IsNullOrWhiteSpace();
}

public interface IChatEmailConnectorSettingsReader
{
    Task<ChatEmailConnectorRuntimeSettings> GetAsync(CancellationToken cancellationToken = default);
}
