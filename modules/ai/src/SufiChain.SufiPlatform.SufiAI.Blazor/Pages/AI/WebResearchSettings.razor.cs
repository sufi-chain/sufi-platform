using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.SufiAI.Permissions;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class WebResearchSettings : IEditableSettingGroup
{
    private readonly SettingGroupEditor<UpdateWebResearchSettingsInput> _edits;
    private bool _loaded;

    public WebResearchSettings()
    {
        _edits = new SettingGroupEditor<UpdateWebResearchSettingsInput>(() => _model, value => _model = value);
    }

    [Inject] private IWebResearchSettingsAppService SettingsService { get; set; } = default!;
    private UpdateWebResearchSettingsInput? _model;
    private bool _busy, _canEdit, _canTest;
    private bool _advancedOpen, _loadFailed;
    private int _validationVersion;
    private Microsoft.AspNetCore.Components.Forms.EditForm? _form;

    public bool IsSaving => _busy;

    public bool HasUnsavedChanges => _edits.HasUnsavedChanges;

    public event Action? EditStateChanged
    {
        add => _edits.Changed += value;
        remove => _edits.Changed -= value;
    }

    /// <summary>
    /// sufi-chain#340 (G-162): save stays on the settings shell, not a native submit button.
    /// A native submit posted the page URL and Blazor answered HTTP 400.
    /// The form stays disabled until the circuit is interactive.
    /// </summary>
    public async Task SaveAsync()
    {
        if (await TrySaveAsync())
        {
            await Message.SuccessAsync(L["WebResearchSaved"]);
        }
    }

    public async Task<bool> TrySaveAsync()
    {
        if (!_loaded || _model == null || !_edits.HasUnsavedChanges)
        {
            return true;
        }

        if (!_canEdit)
        {
            return false;
        }

        var editContext = _form?.EditContext;
        if (editContext != null && !editContext.Validate())
        {
            ShowAdvancedErrors();
            return false;
        }

        return await TrySaveQuietlyAsync(PersistAsync, "save");
    }

    public Task DiscardAsync()
    {
        ClearSaveFieldErrors();
        _edits.Restore();
        return InvokeAsync(StateHasChanged);
    }

    private void ShowAdvancedErrors()
    {
        _advancedOpen = true;
        _validationVersion++;
    }

    protected override async Task OnInitializedAsync()
    {
        _canEdit = await AuthorizationService.IsGrantedAsync(AIPermissions.WebSearchSettings.Update);
        _canTest = await AuthorizationService.IsGrantedAsync(AIPermissions.WebSearchSettings.Test);
        await LoadAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        _edits.Observe();
    }

    private async Task LoadAsync()
    {
        _loadFailed = false;
        try
        {
            _model = JsonSerializer.Deserialize<UpdateWebResearchSettingsInput>(JsonSerializer.Serialize(await SettingsService.GetAsync()));
            _loaded = _model != null;
            if (_loaded)
            {
                _edits.Capture();
            }
        }
        catch (Exception ex) { _loadFailed = true; await HandleErrorAsync(ex); }
    }

    private async Task PersistAsync()
    {
        if (_model == null)
        {
            return;
        }

        _busy = true;
        try
        {
            await SettingsService.UpdateAsync(_model);
            await LoadAsync();
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task TestAsync()
    {
        if (_busy || !_canTest) return;
        _busy = true;
        try
        {
            var result = await SettingsService.TestAsync();
            if (result.Ready) await Message.SuccessAsync(L["WebResearchReady"]);
            else await Message.WarnAsync(L["WebResearchUnavailable"]);
        }
        catch (Exception ex) { await HandleErrorAsync(ex); }
        finally { _busy = false; }
    }
}
