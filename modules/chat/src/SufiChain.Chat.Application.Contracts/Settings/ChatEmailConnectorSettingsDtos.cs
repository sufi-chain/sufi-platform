using Volo.Abp.Application.Services;

namespace SufiChain.Chat.Settings;

public class ChatEmailConnectorSettingsDto
{
    public bool Enabled { get; set; }

    public string? DefaultFromAddress { get; set; }

    public string? ReplyToAddress { get; set; }

    public ChatInboundEmailProtocol InboundProtocol { get; set; }

    public string? InboundHost { get; set; }

    public int InboundPort { get; set; }

    public bool InboundUseSsl { get; set; }

    public string? InboundUserName { get; set; }

    public bool HasInboundPassword { get; set; }

    public string? SmtpHost { get; set; }

    public int SmtpPort { get; set; }

    public bool SmtpUseSsl { get; set; }

    public string? SmtpUserName { get; set; }

    public bool HasSmtpPassword { get; set; }
}

public class UpdateChatEmailConnectorSettingsInput
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
}

public interface IChatEmailConnectorSettingsAppService : IApplicationService
{
    Task<ChatEmailConnectorSettingsDto> GetAsync();

    Task UpdateAsync(UpdateChatEmailConnectorSettingsInput input);
}
