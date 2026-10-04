using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Components;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Models;
using SufiChain.SufiPlatform.Menus.Blazor.Components;
using SufiChain.SufiPlatform.Menus.Menus;
using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.UI.Layout;
using Volo.Abp;

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
    private Dictionary<string, string> _displayNames = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _displayNameBases = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, object> KeyFieldAttributes = new()
    {
        ["dir"] = "ltr",
        ["lang"] = "en"
    };

    private string LabelKeyText => _labelKey ?? string.Empty;
    private List<MultilingualCulture> _labelCultures = new();
    private MultilingualTextField? _labels;
    private string? _labelKey;
    private bool _labelBroken;
    private bool _sameForAll;
    private bool _culturesFailed;
    private string? _saveError;
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
        await LoadCulturesAsync();
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
        _tree.Clear();
        _tree.AddRange(tree);
        _saveError = null;

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
            _parentIdText = parentId?.ToString() ?? string.Empty;
            _displayNames = EmptyNames();
            _displayNameBases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _labelKey = null;
            _labelBroken = false;
            _sameForAll = false;
            _parentOptions = BuildOptions(null);
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
        _parentIdText = item.ParentId?.ToString() ?? string.Empty;
        _parentOptions = BuildOptions(item.Id);
        ApplyLoadedLabel(item.DisplayName, item.Name, item.DisplayNames, item.DisplayNameBases);
        _ready = true;
    }, LoadingKeys.Load, LoadingBehavior.PageProgress);

    private async Task LoadCulturesAsync()
    {
        try
        {
            _labelCultures = MenuLabelCultureMapper.Map(await MenuAppService.GetLabelCulturesAsync());
            _culturesFailed = _labelCultures.Count == 0;
        }
        catch (Exception)
        {
            _labelCultures = new List<MultilingualCulture>();
            _culturesFailed = true;
        }
    }

    private void ApplyLoadedLabel(
        string? stored,
        string? name,
        IReadOnlyDictionary<string, string>? displayNames,
        IReadOnlyDictionary<string, string>? baseValues)
    {
        _labelBroken = BusinessTextEditorStorage.IsPlaceholder(stored);
        _labelKey = BusinessLocalizationHelper.IsBusinessLocalizationKey(stored) && !_labelBroken
            ? stored!.Trim()
            : null;
        _displayNames = EmptyNames();
        _displayNameBases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (baseValues != null)
        {
            foreach (var pair in baseValues)
            {
                _displayNameBases[pair.Key] = pair.Value ?? string.Empty;
            }
        }

        if (displayNames == null)
        {
            return;
        }

        foreach (var pair in displayNames)
        {
            _displayNames[pair.Key] = pair.Value ?? string.Empty;
        }

        if (_labelBroken)
        {
            foreach (var key in _displayNames.Keys.ToList())
            {
                _displayNames[key] = string.Empty;
            }
        }
    }

    private Dictionary<string, string> EmptyNames()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var culture in _labelCultures)
        {
            values[culture.CultureName] = string.Empty;
        }

        if (values.Count == 0)
        {
            values["fa"] = string.Empty;
        }

        return values;
    }

    private Task OnNameChangedAsync(string value)
    {
        _model.Name = value;
        return Task.CompletedTask;
    }

    private Task OnSlugChangedAsync(string value)
    {
        _model.Slug = value;
        return Task.CompletedTask;
    }

    private Task OnDisplayNamesChanged(Dictionary<string, string> values)
    {
        _displayNames = values;
        return Task.CompletedTask;
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        _saveError = null;
        if (_labels == null || !await _labels.ValidateAsync())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_model.Name))
        {
            var defaultCulture = _labelCultures.FirstOrDefault(x => x.IsDefault)?.CultureName ?? "fa";
            if (_displayNames.TryGetValue(defaultCulture, out var label) && !string.IsNullOrWhiteSpace(label))
            {
                _model.Name = label.Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(_model.Name))
        {
            await Message.ErrorAsync(L["NameIsRequired"]);
            return;
        }

        if (!MenuItemUrlRules.IsAcceptable(_model.Kind, _model.Url))
        {
            await Message.ErrorAsync(L["Sufi.Menus:MenuItemInvalidUrl"]);
            return;
        }

        if (!int.TryParse(_displayOrderText, out var displayOrder))
        {
            await Message.ErrorAsync(L["DisplayOrderMustBeNumber"]);
            return;
        }

        _model.DisplayOrder = displayOrder;

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
            await Message.ErrorAsync(L["Menus:MenuItemInvalidTarget"]);
            return;
        }

        _model.DisplayNames = _labels.GetValuesForSave();
        _model.DisplayName = _labelKey ?? _model.Name;

        try
        {
            if (IsCreate)
            {
                _model.MenuId = MenuId;
                await MenuItemAppService.CreateAsync(_model);
                await Notify.SuccessAsync(L["ItemCreatedSuccessfully"]);
            }
            else
            {
                await MenuItemAppService.UpdateAsync(ItemId!.Value, ToUpdateDto());
                await Notify.SuccessAsync(L["ItemUpdatedSuccessfully"]);
            }
        }
        catch (Exception ex) when (ex is BusinessException or UserFriendlyException)
        {
            _saveError = string.IsNullOrWhiteSpace(ex.Message) ? L["Sufi.Menus:DisplayNameRequired"] : ex.Message;
            return;
        }

        GoBack();
    }, LoadingKeys.Save);

    private UpdateMenuItemDto ToUpdateDto() => new()
    {
        ParentId = _model.ParentId,
        Name = _model.Name,
        DisplayName = _model.DisplayName,
        DisplayNames = _model.DisplayNames,
        Slug = _model.Slug,
        Description = _model.Description,
        DisplayOrder = _model.DisplayOrder,
        Kind = _model.Kind,
        DisplayType = _model.DisplayType,
        Url = _model.Url,
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

    private Task CopyLabelKeyAsync() => CopyTextAsync(_labelKey);

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
