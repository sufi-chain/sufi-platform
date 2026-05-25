using SufiChain.SufiAbp.Messaging.Email;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Messaging.Smtp;

public class SmtpEmailSenderConfiguration : IEmailSenderConfiguration, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    public SmtpEmailSenderConfiguration(ISettingProvider settingProvider)
    {
        SettingProvider = settingProvider;
    }

    public virtual async Task<string> GetDefaultFromAddressAsync()
    {
        return await SettingProvider.GetOrNullAsync(MessagingSettingNames.Email.DefaultFromAddress) 
               ?? "noreply@example.com";
    }

    public virtual async Task<string> GetDefaultFromDisplayNameAsync()
    {
        return await SettingProvider.GetOrNullAsync(MessagingSettingNames.Email.DefaultFromDisplayName) 
               ?? "Application";
    }
}
