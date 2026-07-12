using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.SufiCom.Notifications;

/// <summary>
/// Distributed event requesting a voice (TTS) call. Handled by the pro Communication module
/// via the configured voice provider (e.g. Kavenegar). Reserved for critical events.
/// </summary>
[Serializable]
[EventName("Sufi.Communication.SendVoiceNotification")]
public class SendVoiceNotificationEto : NotificationMessageEto
{
    /// <summary>
    /// Recipient phone numbers.
    /// </summary>
    public List<string> To { get; set; } = new();

    /// <summary>
    /// TTS text. Used only when <see cref="TemplateName"/> is not set.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Optional voice-channel template. When set, the handler renders
    /// <see cref="Text"/> from <see cref="TemplateData"/>.
    /// </summary>
    public string? TemplateName { get; set; }

    public Dictionary<string, object>? TemplateData { get; set; }
}
