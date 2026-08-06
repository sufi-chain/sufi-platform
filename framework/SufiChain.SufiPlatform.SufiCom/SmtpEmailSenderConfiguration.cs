using SufiChain.SufiPlatform.SufiCom.Email;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.SufiCom.Smtp;

public class SmtpEmailSenderConfiguration : IEmailSenderConfiguration
{
    protected ISettingProvider SettingProvider { get; }

    public SmtpEmailSenderConfiguration(ISettingProvider settingProvider)
    {
        SettingProvider = settingProvider;
    }

    public virtual async Task<string> GetDefaultFromAddressAsync()
    {
        return await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.DefaultFromAddress) 
               ?? "noreply@example.com";
    }

    public virtual async Task<string> GetDefaultFromDisplayNameAsync()
    {
        return await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.DefaultFromDisplayName) 
               ?? "Application";
    }
}
