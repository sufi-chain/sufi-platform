using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Smtp;

namespace SufiChain.SufiPlatform.Settings;

[Microsoft.AspNetCore.Authorization.Authorize(SettingsPermissions.Emailing)]
public class EmailSettingsAppService : SettingsAppServiceBase, IEmailSettingsAppService
{
    protected ISettingManager SettingManager { get; }
    protected ISmtpEmailSender SmtpEmailSender { get; }

    public EmailSettingsAppService(ISettingManager settingManager, ISmtpEmailSender smtpEmailSender)
    {
        SettingManager = settingManager;
        SmtpEmailSender = smtpEmailSender;
    }

    public virtual async Task<EmailSettingsDto> GetAsync()
    {
        await CheckFeatureAsync();

        var settingsDto = new EmailSettingsDto
        {
            SmtpHost = await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpHost),
            SmtpPort = ToInt32(await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpPort)),
            SmtpUserName = await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpUserName),
            SmtpDomain = await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpDomain),
            SmtpEnableSsl = ToBoolean(await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpEnableSsl)),
            SmtpUseDefaultCredentials = ToBoolean(await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpUseDefaultCredentials)),
            DefaultFromAddress = await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.DefaultFromAddress),
            DefaultFromDisplayName = await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.DefaultFromDisplayName),
        };

        if (CurrentTenant.IsAvailable)
        {
            settingsDto.SmtpHost = await SettingManager.GetOrNullForTenantAsync(SufiComSenderSettingNames.Email.SmtpHost, CurrentTenant.Id!.Value, false);
            settingsDto.SmtpUserName = await SettingManager.GetOrNullForTenantAsync(SufiComSenderSettingNames.Email.SmtpUserName, CurrentTenant.Id!.Value, false);
            settingsDto.SmtpDomain = await SettingManager.GetOrNullForTenantAsync(SufiComSenderSettingNames.Email.SmtpDomain, CurrentTenant.Id!.Value, false);
        }

        return settingsDto;
    }

    public virtual async Task UpdateAsync(UpdateEmailSettingsDto input)
    {
        await CheckFeatureAsync();

        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, SufiComSenderSettingNames.Email.SmtpHost, input.SmtpHost);
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, SufiComSenderSettingNames.Email.SmtpPort, input.SmtpPort.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, SufiComSenderSettingNames.Email.SmtpUserName, input.SmtpUserName);

        if (!string.IsNullOrWhiteSpace(input.SmtpPassword))
        {
            await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, SufiComSenderSettingNames.Email.SmtpPassword, input.SmtpPassword);
        }

        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, SufiComSenderSettingNames.Email.SmtpDomain, input.SmtpDomain);
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, SufiComSenderSettingNames.Email.SmtpEnableSsl, input.SmtpEnableSsl.ToString().ToLowerInvariant());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, SufiComSenderSettingNames.Email.SmtpUseDefaultCredentials, input.SmtpUseDefaultCredentials.ToString().ToLowerInvariant());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, SufiComSenderSettingNames.Email.DefaultFromAddress, input.DefaultFromAddress);
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, SufiComSenderSettingNames.Email.DefaultFromDisplayName, input.DefaultFromDisplayName);
    }

    [Microsoft.AspNetCore.Authorization.Authorize(SettingsPermissions.EmailingTest)]
    public virtual async Task SendTestEmailAsync(SendTestEmailInput input)
    {
        await CheckFeatureAsync();

        var smtpHost = await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpHost);
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            throw new ApplicationException(L["SmtpNotConfigured"]);
        }

        try
        {
            await SendEmailByRegisteredSenderAsync(input);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error sending test email.");
            throw new ApplicationException(L["MailSendingFailed"]);
        }
    }

    protected virtual async Task CheckFeatureAsync()
    {
        if (!await FeatureChecker.IsEnabledAsync(SettingsFeatures.Enable))
        {
            throw new ApplicationException($"Feature is disabled: {SettingsFeatures.Enable}");
        }

        if (CurrentTenant.IsAvailable &&
            !await FeatureChecker.IsEnabledAsync(SettingsFeatures.AllowChangingEmailSettings))
        {
            throw new ApplicationException($"Feature is disabled: {SettingsFeatures.AllowChangingEmailSettings}");
        }
    }

    protected virtual Task SendEmailByRegisteredSenderAsync(SendTestEmailInput input)
    {
        return SmtpEmailSender.SendAsync(
            to: input.TargetEmailAddress,
            from: input.SenderEmailAddress,
            subject: input.Subject,
            body: input.Body ?? string.Empty);
    }

    protected virtual int ToInt32(string? value)
    {
        return int.TryParse(value, out var result) ? result : 0;
    }

    protected virtual bool ToBoolean(string? value)
    {
        return bool.TryParse(value, out var result) && result;
    }
}
