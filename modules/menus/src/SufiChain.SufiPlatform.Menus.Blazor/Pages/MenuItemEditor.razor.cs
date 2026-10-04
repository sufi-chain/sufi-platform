using System.Linq;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Components;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Models;
using SufiChain.SufiPlatform.Menus.Blazor.Components;
using SufiChain.SufiPlatform.Menus.Menus;
using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.Menus.Blazor.Pages;

public partial class MenuItemEditor : MenusComponentBase
{
    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    [Parameter] public Guid MenuId { get; set; }

    [Parameter] public Guid? ItemId { get; set; }

    [SupplyParameterFromQuery(Name = "parentId")]
    public Guid? ParentId { get; set; }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    private IMenuAppService MenuAppService => LazyGetRequiredService(ref _menuAppService);
    private IMenuAppService? _menuAppService;

    private IMenuItemAppService MenuItemAppService => LazyGetRequiredService(ref _menuItemAppService);
    private IMenuItemAppService? _menuItemAppService;

    private readonly List<MenuItemTreeDto> _tree = new();
    private List<MenuItemOption> _parentOptions = new();
    private CreateMenuItemDto _model = new();
    private string _displayOrderText = "0";
    private string _targetIdText = string.Empty;
    private string _parentIdText = string.Empty;
    private BusinessTextEditorMode _displayNameMode = BusinessTextEditorMode.Literal;
    private string? _localizationResourceName;
    private string? _localizationKey;
    private string? _literalDisplayName;
    private BusinessTextEditor? _displayNameEditor;
    private string? _menuKey;
    private string? _menuContextType;
    private string? _loadKey;
    private int _loadVersion;
    private bool _ready;
    private bool _loadFailed;

    private bool IsCreate => !ItemId.HasValue;

    private string ItemsRoute => $"/panel/admin/menu-management/menus/{MenuId}/items";

    protected override async Task OnParametersSetAsync()
    {
        var key = IsCreate
            ? $"{MenuId:N}|new|{ParentId?.ToString("N") ?? string.Empty}"
            : $"{MenuId:N}|{ItemId!.Value:N}";
        if (key == _loadKey)
        {
            return;
        }

        _loadKey = key;
        PageLayout.Title = IsCreate ? L["Menus:CreateMenuItem"] : L["Menus:EditMenuItem"];
        _ready = false;
        _loadFailed = false;
        var version = ++_loadVersion;
        await LoadAsync(version);
        if (version == _loadVersion)
        {
            _loadFailed = !_ready;
        }
    }

    private Task LoadAsync(int version) => ExecuteWithLoadingAsync(async () =>
    {
        var menuId = MenuId;
        var itemId = ItemId;
        var parentId = ParentId;
        var isCreate = !itemId.HasValue;

        var menu = await MenuAppService.GetAsync(menuId);
        var tree = await MenuItemAppService.GetTreeAsync(new GetMenuTreeInput
        {
            MenuId = menuId
        });
        MenuItemDto? item = null;
        if (!isCreate)
        {
            item = await MenuItemAppService.GetAsync(itemId!.Value);
        }

        if (version != _loadVersion)
        {
            return;
        }

        _menuContextType = menu.ContextType;
        _menuKey = MenuLocalizationKeyHelper.ResolveMenuKey(menu.DisplayName, menu.ContextType, menu.Name);
        _localizationResourceName = MenuLocalizationRegistry.GetResourceName(_menuKey, menu.ContextType);
        _tree.Clear();
        _tree.AddRange(tree);

        await LoadLinkCulturesAsync();
        if (version != _loadVersion)
        {
            return;
        }

        if (isCreate)
        {
            _model = new CreateMenuItemDto
            {
                MenuId = menuId,
                ParentId = parentId,
                IsActive = true,
                IsVisible = true
            };
            _displayOrderText = "0";
            _targetIdText = string.Empty;
            ResetLinkEditor(null);
            _parentIdText = parentId?.ToString() ?? string.Empty;
            _displayNameMode = string.Equals(_menuContextType, "Public", StringComparison.OrdinalIgnoreCase)
                ? BusinessTextEditorMode.Localized
                : BusinessTextEditorMode.Literal;
            _literalDisplayName = string.Empty;
            _localizationKey = null;
            _parentOptions = BuildOptions(null);
            UpdateLocalizationBinding();
            _ready = true;
            return;
        }

        if (item == null || item.MenuId != menuId)
        {
            GoBack();
            return;
        }

        _model = new CreateMenuItemDto
        {
            MenuId = item.MenuId,
            ParentId = item.ParentId,
            Name = item.Name,
            DisplayName = item.DisplayName,
            Slug = item.Slug,
            Description = item.Description,
            DisplayOrder = item.DisplayOrder,
            Kind = item.Kind,
            DisplayType = item.DisplayType,
            Url = item.Url,
            CultureUrls = item.CultureUrls?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase),
            LinkTarget = item.LinkTarget,
            TargetType = item.TargetType,
            TargetId = item.TargetId,
            Icon = item.Icon,
            CssClass = item.CssClass,
            PermissionName = item.PermissionName,
            ComponentName = item.ComponentName,
            IsActive = item.IsActive,
            IsVisible = item.IsVisible
        };
        _displayOrderText = item.DisplayOrder.ToString();
        _targetIdText = item.TargetId?.ToString() ?? string.Empty;
        ResetLinkEditor(item);
        _parentIdText = item.ParentId?.ToString() ?? string.Empty;
        _parentOptions = BuildOptions(item.Id);
        InitializeDisplayNameEditor(item);
        _ready = true;
    }, LoadingKeys.Load, LoadingBehavior.PageProgress);

    private void InitializeDisplayNameEditor(MenuItemDto item)
    {
        if (BusinessLocalizationHelper.IsBusinessLocalizationKey(item.DisplayName))
        {
            _displayNameMode = BusinessTextEditorMode.Localized;
            _localizationKey = item.DisplayName;
            _literalDisplayName = string.Empty;
            return;
        }

        _displayNameMode = BusinessTextEditorMode.Literal;
        _literalDisplayName = item.DisplayName;
        _localizationKey = null;
    }

    private Task OnNameChangedAsync(string value)
    {
        _model.Name = value;
        UpdateLocalizationBinding();
        return Task.CompletedTask;
    }

    private Task OnSlugChangedAsync(string value)
    {
        _model.Slug = value;
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
        _localizationResourceName = MenuLocalizationRegistry.GetResourceName(_menuKey, _menuContextType);

        if (!IsCreate)
        {
            if (_displayNameMode != BusinessTextEditorMode.Localized
                || string.IsNullOrWhiteSpace(_menuKey))
            {
                return;
            }

            if (BusinessLocalizationHelper.IsBusinessLocalizationKey(_localizationKey))
            {
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(_menuKey))
        {
            _localizationKey = null;
            return;
        }

        var itemSlug = MenuLocalizationKeyHelper.NormalizeItemSlug(_model.Slug, _model.Name);
        if (string.IsNullOrWhiteSpace(itemSlug))
        {
            _localizationKey = null;
            return;
        }

        _localizationKey = BusinessLocalizationKeys.SeededMenuItemDisplayName(_menuKey, itemSlug);
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (_displayNameEditor == null || !await _displayNameEditor.ValidateAsync())
        {
            return;
        }

        _nameError = null;
        _displayOrderError = null;
        _targetIdError = null;
        _linkError = null;
        var invalid = false;

        if (string.IsNullOrWhiteSpace(_model.Name))
        {
            _nameError = L["NameIsRequired"];
            invalid = true;
        }

        if (!TryApplyLinkChoice())
        {
            invalid = true;
        }

        if (!TryApplyCultureUrls())
        {
            invalid = true;
        }

        if (!MenuItemUrlRules.TryParseDisplayOrder(_displayOrderText, out var displayOrder))
        {
            _displayOrderError = L["DisplayOrderMustBeNumber"];
            invalid = true;
        }
        else
        {
            _model.DisplayOrder = displayOrder;
        }

        if (invalid)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_parentIdText))
        {
            _model.ParentId = null;
        }
        else if (Guid.TryParse(_parentIdText, out var parentId))
        {
            _model.ParentId = parentId;
        }

        if (string.IsNullOrWhiteSpace(_targetIdText))
        {
            _model.TargetId = null;
        }
        else if (Guid.TryParse(_targetIdText, out var targetId))
        {
            _model.TargetId = targetId;
        }
        else
        {
            _targetIdError = L["Menus:MenuItemInvalidTarget"];
            return;
        }

        _model.DisplayName = _displayNameEditor.GetStoredValue();

        if (IsCreate)
        {
            _model.MenuId = MenuId;
            await MenuItemAppService.CreateAsync(_model);
            await _displayNameEditor.SaveAsync();
            await Notify.SuccessAsync(L["ItemCreatedSuccessfully"]);
        }
        else
        {
            await MenuItemAppService.UpdateAsync(ItemId!.Value, ToUpdateDto());
            await _displayNameEditor.SaveAsync();
            await Notify.SuccessAsync(L["ItemUpdatedSuccessfully"]);
        }

        GoBack();
    }, LoadingKeys.Save);

    private UpdateMenuItemDto ToUpdateDto() => new()
    {
        ParentId = _model.ParentId,
        Name = _model.Name,
        DisplayName = _model.DisplayName,
        Slug = _model.Slug,
        Description = _model.Description,
        DisplayOrder = _model.DisplayOrder,
        Kind = _model.Kind,
        DisplayType = _model.DisplayType,
        Url = _model.Url,
        CultureUrls = _model.CultureUrls,
        LinkTarget = _model.LinkTarget,
        TargetType = _model.TargetType,
        TargetId = _model.TargetId,
        Icon = _model.Icon,
        CssClass = _model.CssClass,
        PermissionName = _model.PermissionName,
        ComponentName = _model.ComponentName,
        IsActive = _model.IsActive,
        IsVisible = _model.IsVisible
    };

    private void GoBack() => NavigationManager.NavigateTo(ItemsRoute);

    private List<MenuItemOption> BuildOptions(Guid? excludeId)
    {
        var options = new List<MenuItemOption>();
        foreach (var root in _tree)
        {
            BuildOptionsRecursive(root, string.Empty, excludeId, options);
        }

        return options;
    }

    private void BuildOptionsRecursive(MenuItemTreeDto item, string parentPath, Guid? excludeId, List<MenuItemOption> options)
    {
        if (excludeId.HasValue && item.Id == excludeId.Value)
        {
            return;
        }

        var path = string.IsNullOrEmpty(parentPath)
            ? ResolveMenuItemDisplayName(item, _menuContextType)
            : $"{parentPath} / {ResolveMenuItemDisplayName(item, _menuContextType)}";
        options.Add(new MenuItemOption(item.Id, path));

        foreach (var child in item.Children)
        {
            BuildOptionsRecursive(child, path, excludeId, options);
        }
    }
}
