using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.SettingManagement.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using Volo.Abp.Emailing;
using SufiChain.SufiAbp.SettingManagement;

namespace SufiChain.SufiAbp.SettingManagement.Blazor.Settings;

/// <summary>
/// Email settings group component.
/// Note: This component uses IEmailSettingsAppService (Application Layer) for settings management.
/// Tenant-specific settings should be managed through proper application services, not domain services.
/// </summary>
public partial class EmailSettingsGroup : SettingManagementComponentBase, ISaveableSettingGroup
{

    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
        public const string SendTest = "send-test";
    }

    private IEmailSettingsAppService EmailSettingsAppService => LazyGetRequiredService(ref _emailSettingsAppService);
    private IEmailSettingsAppService? _emailSettingsAppService;

    private EmailSettingsDto _settings = new();
    private bool _showTestEmailModal;
    private string _testEmailAddress = "";

    /// <summary>
    /// Gets a value indicating whether the save operation is currently in progress.
    /// Implements ISaveableSettingGroup.IsSaving.
    /// </summary>
    public bool IsSaving => IsOperationLoading(LoadingKeys.Save);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await LoadSettingsAsync();
        }
    }

    private Task LoadSettingsAsync() => ExecuteWithLoadingAsync(async () =>
    {
        // Load settings for current context using Application Service (proper DDD approach)
        _settings = await EmailSettingsAppService.GetAsync();
    }, LoadingKeys.Load);

    /// <summary>
    /// Saves the email settings.
    /// Implements ISaveableSettingGroup.SaveAsync for centralized save from modal/page footer.
    /// </summary>
    public Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
    {
        // Save settings using Application Service (proper DDD approach)
        await EmailSettingsAppService.UpdateAsync(new UpdateEmailSettingsDto
        {
            SmtpHost = _settings.SmtpHost,
            SmtpPort = _settings.SmtpPort,
            SmtpUserName = _settings.SmtpUserName,
            SmtpPassword = _settings.SmtpPassword,
            SmtpDomain = _settings.SmtpDomain,
            SmtpEnableSsl = _settings.SmtpEnableSsl,
            SmtpUseDefaultCredentials = _settings.SmtpUseDefaultCredentials,
            DefaultFromAddress = _settings.DefaultFromAddress,
            DefaultFromDisplayName = _settings.DefaultFromDisplayName
        });
        
        await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
    }, LoadingKeys.Save);

    private void ShowTestEmailModal()
    {
        _testEmailAddress = "";
        _showTestEmailModal = true;
    }

    private Task SendTestEmailAsync() => ExecuteWithLoadingAsync(async () =>
    {
        await EmailSettingsAppService.SendTestEmailAsync(new SendTestEmailInput
        {
            SenderEmailAddress = _settings.DefaultFromAddress ?? "",
            TargetEmailAddress = _testEmailAddress,
            Subject = L["TestEmailSubject"],
            Body = L["TestEmailBody"]
        });
        
        _showTestEmailModal = false;
        await Notify.SuccessAsync(L["TestEmailSentSuccessfully"]);
    }, LoadingKeys.SendTest);
}
