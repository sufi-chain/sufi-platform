using SufiChain.SufiPlatform.Settings;
using Volo.Abp.Features;

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

    private IFeatureChecker FeatureChecker => LazyGetRequiredService(ref _featureChecker);
    private IFeatureChecker? _featureChecker;

    private EmailSettingsDto _settings = new();
    private string _testEmailAddress = "";
    private bool _lockedByHost;

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

    protected override void OnInitialized()
    {
        // The first paint is before OnAfterRenderAsync starts the read (#584).
        LoadingStates[LoadingKeys.Load] = true;
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
        // The feature is already on the client. A locked tenant must not call the email
        // endpoint: that call used to 500 and sat on the loading skeleton until it failed.
        if (CurrentTenant.IsAvailable &&
            !await FeatureChecker.IsEnabledAsync(SettingsFeatures.AllowChangingEmailSettings))
        {
            _lockedByHost = true;
            _settings = new EmailSettingsDto { LockedByHost = true };
            _edits.Capture();
            return;
        }

        _lockedByHost = false;
        _settings = await EmailSettingsAppService.GetAsync();
        _settings.SmtpPassword = null;
        _lockedByHost = _settings.LockedByHost;
        _edits.Capture();
    }, LoadingKeys.Load);

    /// <summary>
    /// Saves the email settings and shows the modal success toast.
    /// </summary>
    public async Task SaveAsync()
    {
        if (_lockedByHost)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await UpdateSettingsAsync();
            _edits.Capture();
            await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
        }, LoadingKeys.Save);
    }

    public Task<bool> TrySaveAsync()
    {
        if (_lockedByHost)
        {
            return Task.FromResult(false);
        }

        return TrySaveQuietlyAsync(async () =>
        {
            await UpdateSettingsAsync();
            _edits.Capture();
        }, LoadingKeys.Save);
    }

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
