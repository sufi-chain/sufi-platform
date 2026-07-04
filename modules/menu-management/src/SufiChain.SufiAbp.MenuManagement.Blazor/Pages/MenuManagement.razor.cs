using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;
using SufiChain.SufiAbp.UI.Layout;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Pages;

public partial class MenuManagement : MenuManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadMenus = "load-menus";
        public const string DeleteMenu = "delete-menu";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    private IMenuAppService MenuAppService => LazyGetRequiredService(ref _menuAppService);
    private IMenuAppService? _menuAppService;

    private List<MenuListDto> _menus = new();
    private string? _keyword;
    private int _pageIndex = 0;
    private int _pageSize = 10;
    private long _totalCount;

    private bool _showCreateModal;
    private bool _showEditModal;
    private MenuDto? _selectedMenu;

    protected override void OnInitialized()
    {
        PageLayout.Title = L["Menus"];
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await LoadMenusAsync();
        }
    }

    private Task LoadMenusAsync() => ExecuteWithLoadingAsync(async () =>
    {
        var result = await MenuAppService.GetListAsync(new GetMenusInput
        {
            Keyword = _keyword,
            SkipCount = _pageIndex * _pageSize,
            MaxResultCount = _pageSize
        });

        _menus = result.Items.ToList();
        _totalCount = result.TotalCount;
    }, LoadingKeys.LoadMenus);

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await LoadMenusAsync();
    }

    private void ShowCreateModal()
    {
        _showCreateModal = true;
    }

    private async Task ShowEditModal(MenuListDto menu)
    {
        var full = await MenuAppService.GetAsync(menu.Id);
        _selectedMenu = full;
        _showEditModal = true;
    }

    private void GoToItems(MenuListDto menu)
    {
        NavigationManager.NavigateTo($"/panel/admin/menu-management/menus/{menu.Id}/items");
    }

    private void SetCreateOpen(bool open) => _showCreateModal = open;
    private void SetEditOpen(bool open) => _showEditModal = open;

    private async Task OnMenuCreatedAsync()
    {
        _showCreateModal = false;
        await Notify.SuccessAsync(L["MenuCreatedSuccessfully"]);
        await LoadMenusAsync();
    }

    private async Task OnMenuUpdatedAsync()
    {
        _showEditModal = false;
        await Notify.SuccessAsync(L["MenuUpdatedSuccessfully"]);
        await LoadMenusAsync();
    }

    private async Task DeleteMenuAsync(MenuListDto menu)
    {
        if (!await Message.ConfirmAsync(L["DeleteMenuConfirmation", menu.Name]))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await MenuAppService.DeleteAsync(menu.Id);
            await Notify.SuccessAsync(L["MenuDeletedSuccessfully"]);
            await LoadMenusAsync();
        }, LoadingKeys.DeleteMenu);
    }
}
