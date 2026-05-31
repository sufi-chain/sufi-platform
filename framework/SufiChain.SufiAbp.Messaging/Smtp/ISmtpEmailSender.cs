using SufiChain.SufiAbp.Messaging.Email;

namespace SufiChain.SufiAbp.Messaging.Smtp;

/// <summary>
/// Sends emails over SMTP using settings from <see cref="MessagingSettingNames.Email"/>.
/// </summary>
public interface ISmtpEmailSender : IEmailSender;
