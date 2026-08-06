using SufiChain.SufiPlatform.SufiCom.Email;

namespace SufiChain.SufiPlatform.SufiCom.Smtp;

/// <summary>
/// Sends emails over SMTP using settings from <see cref="SufiComSenderSettingNames.Email"/>.
/// </summary>
public interface ISmtpEmailSender : IEmailSender;
