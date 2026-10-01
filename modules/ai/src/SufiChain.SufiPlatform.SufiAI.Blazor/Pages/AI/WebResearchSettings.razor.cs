using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class WebResearchSettings
{
    [Inject] private IWebResearchSettingsAppService SettingsService { get; set; } = default!;
    private UpdateWebResearchSettingsInput? _model;
    private bool _busy, _canEdit, _canTest;
    private bool _advancedOpen, _loadFailed;
    private int _validationVersion;

    private void ShowAdvancedErrors()
    {
        _advancedOpen = true;
        _validationVersion++;
    }

    protected override async Task OnInitializedAsync()
    {
        _canEdit = await AuthorizationService.IsGrantedAsync("SufiAI.WebSearchSettings.Update");
        _canTest = await AuthorizationService.IsGrantedAsync("SufiAI.WebSearchSettings.Test");
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loadFailed = false;
        try { _model = JsonSerializer.Deserialize<UpdateWebResearchSettingsInput>(JsonSerializer.Serialize(await SettingsService.GetAsync())); }
        catch (Exception ex) { _loadFailed = true; await HandleErrorAsync(ex); }
    }

    private async Task SaveAsync()
    {
        if (_model == null || _busy || !_canEdit) return;
        _busy = true;
        try { await SettingsService.UpdateAsync(_model); await LoadAsync(); await Message.SuccessAsync(L["WebResearchSaved"]); }
        catch (Exception ex) { await HandleErrorAsync(ex); }
        finally { _busy = false; }
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
