using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Messaging.Email;

public class EmailSenderConfiguration : IEmailSenderConfiguration, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    public EmailSenderConfiguration(ISettingProvider settingProvider)
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
