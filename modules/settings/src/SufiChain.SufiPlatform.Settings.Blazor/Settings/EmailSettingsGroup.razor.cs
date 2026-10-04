using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

/// <summary>
/// Email settings group component.
/// Note: This component uses IEmailSettingsAppService (Application Layer) for settings management.
/// Tenant-specific settings should be managed through proper application services, not domain services.
/// </summary>
public partial class EmailSettingsGroup : SettingsComponentBase, IEditableSettingGroup
{
    private readonly SettingGroupEditor<EmailSettingsDto> _edits;

    public EmailSettingsGroup()
    {
        _edits = new SettingGroupEditor<EmailSettingsDto>(() => _settings, value => _settings = value ?? new EmailSettingsDto());
    }

    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
        public const string SendTest = "send-test";
    }

    private IEmailSettingsAppService EmailSettingsAppService => LazyGetRequiredService(ref _emailSettingsAppService);
    private IEmailSettingsAppService? _emailSettingsAppService;

    private EmailSettingsDto _settings = new();
    private string _testEmailAddress = "";

    /// <summary>
    /// Gets a value indicating whether the save operation is currently in progress.
    /// Implements ISaveableSettingGroup.IsSaving.
    /// </summary>
    public bool IsSaving => IsOperationLoading(LoadingKeys.Save);

    public bool HasUnsavedChanges => _edits.HasUnsavedChanges;

    public event Action? EditStateChanged
    {
        add => _edits.Changed += value;
        remove => _edits.Changed -= value;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        _edits.Observe();
    }

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
        _settings = await EmailSettingsAppService.GetAsync();
        _edits.Capture();
    }, LoadingKeys.Load);

    /// <summary>
    /// Saves the email settings and shows the modal success toast.
    /// </summary>
    public async Task SaveAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await UpdateSettingsAsync();
            _edits.Capture();
            await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
        }, LoadingKeys.Save);
    }

    public Task<bool> TrySaveAsync() =>
        TrySaveQuietlyAsync(async () =>
        {
            await UpdateSettingsAsync();
            _edits.Capture();
        }, LoadingKeys.Save);

    public Task DiscardAsync()
    {
        ClearSaveFieldErrors();
        _edits.Restore();
        return InvokeAsync(StateHasChanged);
    }

    private Task SendTestEmailAsync() => ExecuteWithLoadingAsync(async () =>
    {
        await UpdateSettingsAsync();
        _edits.Capture();
        await EmailSettingsAppService.SendTestEmailAsync(new SendTestEmailInput
        {
            SenderEmailAddress = _settings.DefaultFromAddress ?? "",
            TargetEmailAddress = _testEmailAddress,
            Subject = L["TestEmailSubject"],
            Body = L["TestEmailBody"]
        });

        await Notify.SuccessAsync(L["TestEmailSentSuccessfully"]);
    }, LoadingKeys.SendTest);

    private async Task UpdateSettingsAsync()
    {
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

        _settings.SmtpPassword = null;
    }
}
