using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Data;
using SufiChain.SufiAbp.LocalizationManagement.Blazor.Public.Components;
using SufiChain.SufiAbp.LocalizationManagement.Blazor.Public.Models;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuEditModal : MenuManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string Save = "save";
    }

    private IMenuAppService MenuAppService => LazyGetRequiredService(ref _menuAppService);
    private IMenuAppService? _menuAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public MenuDto? Menu { get; set; }
    [Parameter] public EventCallback OnMenuUpdated { get; set; }

    private UpdateMenuDto _model = new();
    private Guid _menuId;
    private BusinessTextEditorMode _displayNameMode = BusinessTextEditorMode.Literal;
    private string? _localizationResourceName;
    private string? _localizationKey;
    private string? _literalDisplayName;
    private BusinessTextEditor? _displayNameEditor;

    protected override void OnParametersSet()
    {
        if (Open && Menu != null && Menu.Id != _menuId)
        {
            _menuId = Menu.Id;
            _model = new UpdateMenuDto
            {
                DisplayName = Menu.DisplayName,
                Description = Menu.Description,
                IsActive = Menu.IsActive
            };

            InitializeDisplayNameEditor(Menu);
        }
    }

    private void InitializeDisplayNameEditor(MenuDto menu)
    {
        if (BusinessLocalizationHelper.IsBusinessLocalizationKey(menu.DisplayName))
        {
            _displayNameMode = BusinessTextEditorMode.Localized;
            _localizationKey = menu.DisplayName;
            _literalDisplayName = string.Empty;

            var menuKey = MenuLocalizationKeyHelper.ResolveMenuKey(menu.DisplayName, menu.ContextType, menu.Name);
            _localizationResourceName = MenuLocalizationRegistry.GetResourceName(menuKey, menu.ContextType);
            return;
        }

        _displayNameMode = BusinessTextEditorMode.Literal;
        _literalDisplayName = menu.DisplayName;
        _localizationKey = null;
        _localizationResourceName = null;
    }

    private Task Hide() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    private Task OnDisplayNameModeChangedAsync(BusinessTextEditorMode mode)
    {
        _displayNameMode = mode;

        if (Menu == null)
        {
            return Task.CompletedTask;
        }

        if (mode == BusinessTextEditorMode.Localized)
        {
            var menuKey = MenuLocalizationKeyHelper.ResolveMenuKey(_localizationKey, Menu.ContextType, Menu.Name);
            if (!string.IsNullOrWhiteSpace(menuKey))
            {
                _localizationKey = BusinessLocalizationKeys.SeededMenuDisplayName(menuKey);
                _localizationResourceName = MenuLocalizationRegistry.GetResourceName(menuKey, Menu.ContextType);
            }
        }

        return Task.CompletedTask;
    }

    private Task OnLiteralDisplayNameChangedAsync(string? value)
    {
        _literalDisplayName = value;
        return Task.CompletedTask;
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (_displayNameEditor == null || !await _displayNameEditor.ValidateAsync())
        {
            return;
        }

        _model.DisplayName = _displayNameEditor.GetStoredValue();
        await MenuAppService.UpdateAsync(_menuId, _model);
        await _displayNameEditor.SaveAsync();
        await OnMenuUpdated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Save);
}
