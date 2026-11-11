using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Smtp;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Features;
using Volo.Abp.Settings;

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

    /// <summary>
    /// Values a caller may read. The encrypted SMTP password is never in this list.
    /// </summary>
    public static readonly string[] ReadableSettingNames =
    [
        SufiComSenderSettingNames.Email.SmtpHost,
        SufiComSenderSettingNames.Email.SmtpPort,
        SufiComSenderSettingNames.Email.SmtpUserName,
        SufiComSenderSettingNames.Email.SmtpDomain,
        SufiComSenderSettingNames.Email.SmtpEnableSsl,
        SufiComSenderSettingNames.Email.SmtpUseDefaultCredentials,
        SufiComSenderSettingNames.Email.DefaultFromAddress,
        SufiComSenderSettingNames.Email.DefaultFromDisplayName
    ];

    public virtual async Task<EmailSettingsDto> GetAsync()
    {
        await FeatureChecker.CheckEnabledAsync(SettingsFeatures.Enable);

        // A tenant with email changes disabled used to throw here. That was a 500 on section
        // load, and the following per-key reads paid for a settings round trip each.
        if (await EmailIsLockedByHostAsync())
        {
            return new EmailSettingsDto { LockedByHost = true };
        }

        var values = await SettingProvider.GetAllAsync(ReadableSettingNames);
        string? Value(string name) => values.FirstOrDefault(value => value.Name == name)?.Value;

        return new EmailSettingsDto
        {
            SmtpHost = Value(SufiComSenderSettingNames.Email.SmtpHost),
            SmtpPort = ToInt32(Value(SufiComSenderSettingNames.Email.SmtpPort)),
            SmtpUserName = Value(SufiComSenderSettingNames.Email.SmtpUserName),
            SmtpDomain = Value(SufiComSenderSettingNames.Email.SmtpDomain),
            SmtpEnableSsl = ToBoolean(Value(SufiComSenderSettingNames.Email.SmtpEnableSsl)),
            SmtpUseDefaultCredentials = ToBoolean(Value(SufiComSenderSettingNames.Email.SmtpUseDefaultCredentials)),
            DefaultFromAddress = Value(SufiComSenderSettingNames.Email.DefaultFromAddress),
            DefaultFromDisplayName = Value(SufiComSenderSettingNames.Email.DefaultFromDisplayName)
        };
    }

    public virtual async Task UpdateAsync(UpdateEmailSettingsDto input)
    {
        await EnsureCanChangeEmailAsync();
        EmailSettingsRules.RejectInvalidFromAddress(input.DefaultFromAddress, L["DefaultFromAddress:Invalid"]);

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
        await EnsureCanChangeEmailAsync();

        var smtpHost = await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpHost);
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            throw new ApplicationException(L["SmtpNotConfigured"]);
        }

        try
        {
            await SendEmailByRegisteredSenderAsync(input);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Error sending test email.");
            var reason = DescribeMailFailure(exception);
            throw new UserFriendlyException(
                string.IsNullOrWhiteSpace(reason)
                    ? L["MailSendingFailed"]
                    : $"{L["MailSendingFailed"]} {reason}");
        }
    }

    protected virtual string DescribeMailFailure(Exception exception)
    {
        var message = exception.GetBaseException().Message?.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            return string.Empty;
        }

        const int maxLength = 240;
        return message.Length <= maxLength ? message : message[..maxLength];
    }

    protected virtual async Task<bool> EmailIsLockedByHostAsync()
    {
        return CurrentTenant.IsAvailable &&
            !await FeatureChecker.IsEnabledAsync(SettingsFeatures.AllowChangingEmailSettings);
    }

    protected virtual async Task EnsureCanChangeEmailAsync()
    {
        await FeatureChecker.CheckEnabledAsync(SettingsFeatures.Enable);
        if (CurrentTenant.IsAvailable)
        {
            await FeatureChecker.CheckEnabledAsync(SettingsFeatures.AllowChangingEmailSettings);
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
