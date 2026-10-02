using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Menus.Blazor.Components;
using SufiChain.SufiPlatform.Menus.Hooshvare;
using SufiChain.SufiPlatform.Menus.Menus;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.Menus.Blazor.Pages;

public partial class MenuItemsManagement : MenusComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadTree = "load-tree";
        public const string DeleteItem = "delete-item";
        public const string Reorder = "reorder";
    }

    [Parameter] public Guid MenuId { get; set; }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    private IMenuAppService MenuAppService => LazyGetRequiredService(ref _menuAppService);
    private IMenuAppService? _menuAppService;

    private IMenuItemAppService MenuItemAppService => LazyGetRequiredService(ref _menuItemAppService);
    private IMenuItemAppService? _menuItemAppService;

    private List<MenuItemTreeDto> _tree = new();
    private string _menuDisplayName = string.Empty;
    private string? _menuContextType;

    private List<MenuItemOption> _parentOptions = new();

    private bool _showMoveModal;

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

    private void GoToNewItem()
    {
        NavigationManager.NavigateTo($"/panel/admin/menu-management/menus/{MenuId}/items/new");
    }

    private Task OnAddChildAsync(MenuItemTreeDto item)
    {
        NavigationManager.NavigateTo($"/panel/admin/menu-management/menus/{MenuId}/items/new?parentId={item.Id}");
        return Task.CompletedTask;
    }

    private Task OnEditAsync(MenuItemTreeDto item)
    {
        NavigationManager.NavigateTo($"/panel/admin/menu-management/menus/{MenuId}/items/{item.Id}");
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

    private void SetMoveOpen(bool open) => _showMoveModal = open;

    private async Task OnItemMovedAsync()
    {
        _showMoveModal = false;
        await Notify.SuccessAsync(L["ItemMovedSuccessfully"]);
        await LoadTreeAsync();
    }

    private IReadOnlyDictionary<string, string> BuildHooshvareContext()
    {
        var context = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [MenusDesignerHooshvareKeys.Context.MenuId] = MenuId.ToString()
        };
        if (!string.IsNullOrWhiteSpace(_menuDisplayName))
        {
            context[MenusDesignerHooshvareKeys.Context.MenuName] = _menuDisplayName;
        }

        if (_selectedItem != null)
        {
            context[MenusDesignerHooshvareKeys.Context.SelectedItemId] = _selectedItem.Id.ToString();
            if (!string.IsNullOrWhiteSpace(_selectedItem.Url) && !_selectedItem.Url.Contains('?'))
            {
                context[MenusDesignerHooshvareKeys.Context.RouteConstraints] = _selectedItem.Url;
            }
        }

        var tree = DescribeMenuTree(_tree);
        if (!string.IsNullOrWhiteSpace(tree))
        {
            context[MenusDesignerHooshvareKeys.Context.Tree] = tree;
        }

        return context;
    }

    private static string DescribeMenuTree(IEnumerable<MenuItemTreeDto> items, int depth = 0)
    {
        var lines = new List<string>();
        foreach (var item in items)
        {
            lines.Add($"{new string(' ', depth * 2)}{item.DisplayName} ({item.Id:D})");
            if (item.Children.Count > 0)
            {
                lines.Add(DescribeMenuTree(item.Children, depth + 1));
            }
        }

        var text = string.Join('\n', lines.Where(line => !string.IsNullOrWhiteSpace(line)));
        return text.Length > 2000 ? text[..2000] : text;
    }
}
