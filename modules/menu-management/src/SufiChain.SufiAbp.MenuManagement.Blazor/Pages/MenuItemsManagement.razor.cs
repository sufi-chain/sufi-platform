using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Blazor.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;
using SufiChain.SufiAbp.UI.Layout;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Pages;

public partial class MenuItemsManagement : MenuManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadTree = "load-tree";
        public const string DeleteItem = "delete-item";
        public const string Reorder = "reorder";
    }

    [Parameter] public Guid MenuId { get; set; }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    private IMenuAppService MenuAppService => LazyGetRequiredService(ref _menuAppService);
    private IMenuAppService? _menuAppService;

    private IMenuItemAppService MenuItemAppService => LazyGetRequiredService(ref _menuItemAppService);
    private IMenuItemAppService? _menuItemAppService;

    private List<MenuItemTreeDto> _tree = new();
    private string _menuDisplayName = string.Empty;
    private string? _menuContextType;
    private string? _menuKey;
    private string? _menuResourceName;

    private List<MenuItemOption> _parentOptions = new();

    private bool _showCreateModal;
    private bool _showEditModal;
    private bool _showMoveModal;

    private Guid? _createParentId;
    private MenuItemTreeDto? _selectedItem;

    protected override async Task OnInitializedAsync()
    {
        PageLayout.Title = L["Items"];
        await LoadMenuAsync();
        await LoadTreeAsync();
    }

    private async Task LoadMenuAsync()
    {
        try
        {
            var menu = await MenuAppService.GetAsync(MenuId);
            _menuContextType = menu.ContextType;
            _menuKey = MenuLocalizationKeyHelper.ResolveMenuKey(menu.DisplayName, menu.ContextType, menu.Name);
            _menuResourceName = MenuLocalizationRegistry.GetResourceName(_menuKey, menu.ContextType);
            _menuDisplayName = ResolveMenuDisplayName(menu);
        }
        catch
        {
            _menuDisplayName = L["Items"];
        }
    }

    private Task LoadTreeAsync() => ExecuteWithLoadingAsync(async () =>
    {
        var tree = await MenuItemAppService.GetTreeAsync(new GetMenuTreeInput
        {
            MenuId = MenuId
        });
        _tree = tree.ToList();
        _parentOptions = BuildOptions(null);
    }, LoadingKeys.LoadTree);

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

    private void ShowCreateRootModal()
    {
        _createParentId = null;
        _parentOptions = BuildOptions(null);
        _showCreateModal = true;
    }

    private Task OnAddChildAsync(MenuItemTreeDto item)
    {
        _createParentId = item.Id;
        _parentOptions = BuildOptions(null);
        _showCreateModal = true;
        return Task.CompletedTask;
    }

    private Task OnEditAsync(MenuItemTreeDto item)
    {
        _selectedItem = item;
        _parentOptions = BuildOptions(item.Id);
        _showEditModal = true;
        return Task.CompletedTask;
    }

    private Task OnMoveAsync(MenuItemTreeDto item)
    {
        _selectedItem = item;
        _parentOptions = BuildOptions(item.Id);
        _showMoveModal = true;
        return Task.CompletedTask;
    }

    private async Task OnDeleteAsync(MenuItemTreeDto item)
    {
        if (!await Message.ConfirmAsync(L["DeleteItemConfirmation"]))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await MenuItemAppService.DeleteAsync(item.Id);
            await Notify.SuccessAsync(L["ItemDeletedSuccessfully"]);
            await LoadTreeAsync();
        }, LoadingKeys.DeleteItem);
    }

    private Task OnReorderUpAsync(MenuItemTreeDto item) => ExecuteWithLoadingAsync(async () =>
    {
        await MenuItemAppService.ReorderAsync(item.Id, item.DisplayOrder - 1);
        await LoadTreeAsync();
    }, LoadingKeys.Reorder);

    private Task OnReorderDownAsync(MenuItemTreeDto item) => ExecuteWithLoadingAsync(async () =>
    {
        await MenuItemAppService.ReorderAsync(item.Id, item.DisplayOrder + 1);
        await LoadTreeAsync();
    }, LoadingKeys.Reorder);

    private void SetCreateOpen(bool open) => _showCreateModal = open;
    private void SetEditOpen(bool open) => _showEditModal = open;
    private void SetMoveOpen(bool open) => _showMoveModal = open;

    private async Task OnItemCreatedAsync()
    {
        _showCreateModal = false;
        await Notify.SuccessAsync(L["ItemCreatedSuccessfully"]);
        await LoadTreeAsync();
    }

    private async Task OnItemUpdatedAsync()
    {
        _showEditModal = false;
        await Notify.SuccessAsync(L["ItemUpdatedSuccessfully"]);
        await LoadTreeAsync();
    }

    private async Task OnItemMovedAsync()
    {
        _showMoveModal = false;
        await Notify.SuccessAsync(L["ItemMovedSuccessfully"]);
        await LoadTreeAsync();
    }
}
