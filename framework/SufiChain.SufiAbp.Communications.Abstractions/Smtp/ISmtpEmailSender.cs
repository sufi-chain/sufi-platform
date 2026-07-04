using SufiChain.SufiAbp.Communications.Email;

namespace SufiChain.SufiAbp.Communications.Smtp;

/// <summary>
/// Sends emails over SMTP using settings from <see cref="CommunicationsSettingNames.Email"/>.
/// </summary>
public interface ISmtpEmailSender : IEmailSender;
