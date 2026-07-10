using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Data;
using SufiChain.SufiAbp.LocalizationManagement.Blazor.Public.Components;
using SufiChain.SufiAbp.LocalizationManagement.Blazor.Public.Models;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuCreateModal : MenuManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string Create = "create";
    }

    private IMenuAppService MenuAppService => LazyGetRequiredService(ref _menuAppService);
    private IMenuAppService? _menuAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public EventCallback OnMenuCreated { get; set; }

    private CreateMenuDto _model = new();
    private string _contextIdText = string.Empty;
    private BusinessTextEditorMode _displayNameMode = BusinessTextEditorMode.Literal;
    private string? _localizationResourceName;
    private string? _localizationKey;
    private string? _literalDisplayName;
    private BusinessTextEditor? _displayNameEditor;

    protected override void OnParametersSet()
    {
        if (Open)
        {
            _model = new CreateMenuDto();
            _contextIdText = string.Empty;
            _displayNameMode = BusinessTextEditorMode.Literal;
            _literalDisplayName = string.Empty;
            _localizationKey = null;
            _localizationResourceName = null;
        }
    }

    private Task Hide() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    private Task OnNameChangedAsync(string value)
    {
        _model.Name = value;
        UpdateLocalizationBinding();
        return Task.CompletedTask;
    }

    private Task OnContextTypeChangedAsync(string value)
    {
        _model.ContextType = value;
        _displayNameMode = string.Equals(_model.ContextType, "Public", StringComparison.OrdinalIgnoreCase)
            ? BusinessTextEditorMode.Localized
            : BusinessTextEditorMode.Literal;
        UpdateLocalizationBinding();
        return Task.CompletedTask;
    }

    private Task OnDisplayNameModeChangedAsync(BusinessTextEditorMode mode)
    {
        _displayNameMode = mode;
        UpdateLocalizationBinding();
        return Task.CompletedTask;
    }

    private Task OnLiteralDisplayNameChangedAsync(string? value)
    {
        _literalDisplayName = value;
        return Task.CompletedTask;
    }

    private void UpdateLocalizationBinding()
    {
        var menuKey = MenuLocalizationKeyHelper.NormalizeMenuKey(_model.ContextType, _model.Name);
        if (string.IsNullOrWhiteSpace(menuKey))
        {
            _localizationKey = null;
            _localizationResourceName = null;
            return;
        }

        _localizationKey = BusinessLocalizationKeys.SeededMenuDisplayName(menuKey);
        _localizationResourceName = MenuLocalizationRegistry.GetResourceName(menuKey, _model.ContextType);
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (_displayNameEditor == null || !await _displayNameEditor.ValidateAsync())
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(_contextIdText))
        {
            if (!Guid.TryParse(_contextIdText, out var contextId))
            {
                await Message.ErrorAsync(L["InvalidEntityId"]);
                return;
            }

            _model.ContextId = contextId;
        }
        else
        {
            _model.ContextId = null;
        }

        _model.DisplayName = _displayNameEditor.GetStoredValue();
        await MenuAppService.CreateAsync(_model);
        await _displayNameEditor.SaveAsync();
        await OnMenuCreated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Create);
}
