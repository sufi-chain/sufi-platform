using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Account;

public class AccountVerificationEventHandler :
    ILocalEventHandler<UserRegisteredEvent>,
    ILocalEventHandler<PasswordResetRequestedEvent>,
    ILocalEventHandler<VerificationCodeRequestedEvent>,
    ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    protected IAppUrlProvider AppUrlProvider { get; }

    protected IVerificationCodeDispatcher VerificationCodeDispatcher { get; }

    public AccountVerificationEventHandler(
        ISettingProvider settingProvider,
        IAppUrlProvider appUrlProvider,
        IVerificationCodeDispatcher verificationCodeDispatcher)
    {
        SettingProvider = settingProvider;
        AppUrlProvider = appUrlProvider;
        VerificationCodeDispatcher = verificationCodeDispatcher;
    }

    public virtual async Task HandleEventAsync(UserRegisteredEvent eventData)
    {
        if (string.IsNullOrWhiteSpace(eventData.EmailConfirmationToken))
        {
            return;
        }

        var requireEmailConfirmation = await SettingProvider.IsTrueAsync(
            IdentitySettingNames.Registration.RequireEmailConfirmation);

        if (!requireEmailConfirmation)
        {
            return;
        }

        var link = await AppUrlProvider.GetEmailConfirmationUrlAsync(
            eventData.AppName,
            eventData.UserId,
            eventData.EmailConfirmationToken,
            eventData.ReturnUrl,
            eventData.ReturnUrlHash);

        await VerificationCodeDispatcher.SendAsync(new VerificationMessage
        {
            Purpose = VerificationPurpose.EmailConfirmation,
            Recipient = eventData.Email,
            Link = link,
            UserId = eventData.UserId,
            AppName = eventData.AppName
        });
    }

    public virtual async Task HandleEventAsync(PasswordResetRequestedEvent eventData)
    {
        var link = await AppUrlProvider.GetPasswordResetUrlAsync(
            eventData.AppName ?? string.Empty,
            eventData.UserId,
            eventData.ResetToken,
            eventData.ReturnUrl,
            eventData.ReturnUrlHash);

        await VerificationCodeDispatcher.SendAsync(new VerificationMessage
        {
            Purpose = VerificationPurpose.PasswordReset,
            Recipient = eventData.Email,
            Link = link,
            UserId = eventData.UserId,
            AppName = eventData.AppName
        });
    }

    public virtual async Task HandleEventAsync(VerificationCodeRequestedEvent eventData)
    {
        await VerificationCodeDispatcher.SendAsync(new VerificationMessage
        {
            Purpose = eventData.Purpose,
            PreferredChannel = eventData.PreferredChannel,
            Recipient = eventData.Identifier,
            Code = eventData.Code,
            UserId = eventData.UserId,
            AppName = eventData.AppName
        });
    }
}
