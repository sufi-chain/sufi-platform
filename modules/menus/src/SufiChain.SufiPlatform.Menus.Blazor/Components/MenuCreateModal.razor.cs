using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Components;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Models;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Menus.Blazor.Components;

public partial class MenuCreateModal : MenusComponentBase
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
    private Dictionary<string, string> _displayNames = new(StringComparer.OrdinalIgnoreCase);
    private List<MultilingualCulture> _labelCultures = new();
    private MultilingualTextField? _labels;
    private bool _sameForAll;
    private bool _culturesFailed;
    private bool _culturesRequested;
    private string? _saveError;
    private List<string> _contextTypes = ["Public"];

    protected override async Task OnParametersSetAsync()
    {
        if (!Open)
        {
            _culturesRequested = false;
            return;
        }

        if (_culturesRequested)
        {
            return;
        }

        _culturesRequested = true;
        _contextTypes = ContextTypeOptions();
        _model = new CreateMenuDto { ContextType = "Public" };
        _contextIdText = string.Empty;
        _sameForAll = false;
        _saveError = null;
        await LoadCulturesAsync();
        _displayNames = EmptyNames();
    }

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

    private Task Hide() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    private Task OnNameChangedAsync(string value)
    {
        _model.Name = value;
        return Task.CompletedTask;
    }

    private static List<string> ContextTypeOptions()
    {
        var types = new List<string> { "Public", "SufiCMS", "SufiHelpDesk.KnowledgeBase.Project" };
        foreach (var registered in MenuLocalizationRegistry.GetContextTypes())
        {
            if (!types.Any(type => string.Equals(type, registered, StringComparison.OrdinalIgnoreCase)))
            {
                types.Add(registered);
            }
        }

        return types;
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

        _model.DisplayNames = _labels.GetValuesForSave();
        _model.DisplayName = _model.Name;
        try
        {
            await MenuAppService.CreateAsync(_model);
        }
        catch (Exception ex) when (ex is BusinessException or UserFriendlyException)
        {
            _saveError = string.IsNullOrWhiteSpace(ex.Message) ? L["Sufi.Menus:DisplayNameRequired"] : ex.Message;
            return;
        }

        await OnMenuCreated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Create);
}
