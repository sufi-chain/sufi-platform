using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;
using System.Text.Json;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuItemCreateModal : MenuManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string Create = "create";
    }

    private IMenuItemAppService MenuItemAppService => LazyGetRequiredService(ref _menuItemAppService);
    private IMenuItemAppService? _menuItemAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid MenuId { get; set; }
    [Parameter] public Guid? ParentId { get; set; }
    [Parameter] public IReadOnlyList<MenuItemOption> ParentOptions { get; set; } = new List<MenuItemOption>();
    [Parameter] public EventCallback OnItemCreated { get; set; }

    private CreateMenuItemDto _model = new();
    private string _displayOrderText = "0";
    private string _targetIdText = string.Empty;
    private string _parentIdText = string.Empty;

    protected override void OnParametersSet()
    {
        if (Open)
        {
            _model = new CreateMenuItemDto
            {
                MenuId = MenuId,
                ParentId = ParentId,
                IsActive = true,
                IsVisible = true
            };
            _displayOrderText = "0";
            _targetIdText = string.Empty;
            _parentIdText = ParentId?.ToString() ?? string.Empty;
        }
    }

    private Task Hide() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(_model.Name))
        {
            await Message.ErrorAsync(L["NameIsRequired"]);
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

        if (!string.IsNullOrWhiteSpace(_targetIdText))
        {
            if (Guid.TryParse(_targetIdText, out var targetId))
            {
                _model.TargetId = targetId;
            }
        }
        else
        {
            _model.TargetId = null;
        }

        if (!string.IsNullOrWhiteSpace(_model.MetadataJson))
        {
            try
            {
                JsonDocument.Parse(_model.MetadataJson);
            }
            catch
            {
                await Message.ErrorAsync(L["InvalidJsonFormat"]);
                return;
            }
        }

        await MenuItemAppService.CreateAsync(_model);
        await OnItemCreated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Create);
}
