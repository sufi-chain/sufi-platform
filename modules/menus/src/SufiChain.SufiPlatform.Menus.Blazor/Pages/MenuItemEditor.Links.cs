using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.Menus.Menus;

namespace SufiChain.SufiPlatform.Menus.Blazor.Pages;

public partial class MenuItemEditor
{
    private MenuLinkChoice _linkChoice = MenuLinkChoice.SitePage;
    private string _structuralKindText = string.Empty;
    private string _pagePath = string.Empty;
    private bool _sectionIsCurrentPage = true;
    private string _sectionPagePath = string.Empty;
    private string _sectionId = string.Empty;
    private string _websiteUrl = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string? _linkError;
    private string? _nameError;
    private string? _displayOrderError;
    private string? _targetIdError;
    private int _errorSummaryCount;
    private bool _focusSummary;
    private bool _openAdvancedOnRender;
    private ElementReference _summaryRef;
    private ElementReference _advancedRef;
    private IJSObjectReference? _editorModule;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    private readonly List<MenuCultureUrlRow> _cultureUrlRows = new();
    private List<MenuLinkCulture> _linkCultures = FallbackCultures();
    private IBusinessLocalizationEditorAppService? _businessLocalizationEditorAppService;

    private IBusinessLocalizationEditorAppService BusinessLocalizationEditorAppService =>
        LazyGetRequiredService(ref _businessLocalizationEditorAppService);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (_openAdvancedOnRender)
        {
            _openAdvancedOnRender = false;
            try
            {
                _editorModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./_content/SufiChain.SufiPlatform.Menus.Blazor/menu-item-editor.js");
                await _editorModule.InvokeVoidAsync("openDetails", _advancedRef);
            }
            catch (Exception)
            {
                // The summary still names the errors when the panel cannot be opened from script.
            }
        }

        if (!_focusSummary)
        {
            return;
        }

        _focusSummary = false;
        try
        {
            await _summaryRef.FocusAsync();
        }
        catch (Exception)
        {
            // The summary is still announced by role="alert" when focus is not available.
        }
    }

    private void PublishValidation(bool displayNameFailed)
    {
        _errorSummaryCount = CountValidationErrors(displayNameFailed);
        _openAdvancedOnRender = HasAdvancedErrors();
        _focusSummary = _errorSummaryCount > 0;
    }

    private int CountValidationErrors(bool displayNameFailed)
    {
        var count = displayNameFailed ? 1 : 0;
        if (!string.IsNullOrEmpty(_nameError))
        {
            count++;
        }

        if (!string.IsNullOrEmpty(_linkError))
        {
            count++;
        }

        if (!string.IsNullOrEmpty(_displayOrderError))
        {
            count++;
        }

        if (!string.IsNullOrEmpty(_targetIdError))
        {
            count++;
        }

        count += _cultureUrlRows.Count(row => !string.IsNullOrEmpty(row.Error));
        return count;
    }

    private bool HasAdvancedErrors() =>
        !string.IsNullOrEmpty(_targetIdError) || _cultureUrlRows.Any(row => !string.IsNullOrEmpty(row.Error));

    private void OnLinkChoiceChanged(MenuLinkChoice choice)
    {
        _linkChoice = choice;
        _structuralKindText = string.Empty;
        _linkError = null;
        if (_linkChoice == MenuLinkChoice.Website)
        {
            _model.LinkTarget = MenuLinkTarget.NewTab;
        }
    }

    private void AddCultureUrlRow()
    {
        var used = new HashSet<string>(_cultureUrlRows.Select(row => row.Culture), StringComparer.OrdinalIgnoreCase);
        var next = _linkCultures.FirstOrDefault(culture => !used.Contains(culture.CultureName))?.CultureName
            ?? _linkCultures.FirstOrDefault()?.CultureName
            ?? "fa";
        _cultureUrlRows.Add(new MenuCultureUrlRow { Culture = next });
    }

    private void RemoveCultureUrlRow(MenuCultureUrlRow row) => _cultureUrlRows.Remove(row);

    private IEnumerable<MenuLinkCulture> CultureOptions(MenuCultureUrlRow row)
    {
        if (_linkCultures.Any(culture => string.Equals(culture.CultureName, row.Culture, StringComparison.OrdinalIgnoreCase)))
        {
            return _linkCultures;
        }

        return _linkCultures.Prepend(new MenuLinkCulture
        {
            CultureName = row.Culture,
            DisplayName = string.IsNullOrWhiteSpace(row.Culture) ? row.Culture : row.Culture
        });
    }

    private async Task LoadLinkCulturesAsync()
    {
        try
        {
            var cultures = await BusinessLocalizationEditorAppService.GetEditorCulturesAsync();
            var mapped = cultures
                .OrderByDescending(culture => culture.IsDefault)
                .ThenBy(culture => culture.CultureName, StringComparer.OrdinalIgnoreCase)
                .Select(culture => new MenuLinkCulture
                {
                    CultureName = culture.CultureName,
                    DisplayName = string.IsNullOrWhiteSpace(culture.DisplayName) ? culture.CultureName : culture.DisplayName
                })
                .Where(culture => !string.IsNullOrWhiteSpace(culture.CultureName))
                .ToList();
            if (mapped.Count > 0)
            {
                _linkCultures = mapped;
                return;
            }
        }
        catch (Exception)
        {
            // The link table still works with the built-in culture list when the editor cultures cannot be loaded.
        }

        _linkCultures = FallbackCultures();
    }

    private void ResetLinkEditor(MenuItemDto? item)
    {
        _linkError = null;
        _nameError = null;
        _displayOrderError = null;
        _targetIdError = null;
        _errorSummaryCount = 0;
        _structuralKindText = string.Empty;
        _pagePath = string.Empty;
        _sectionIsCurrentPage = true;
        _sectionPagePath = string.Empty;
        _sectionId = string.Empty;
        _websiteUrl = string.Empty;
        _email = string.Empty;
        _phone = string.Empty;
        _cultureUrlRows.Clear();

        if (item == null)
        {
            _linkChoice = MenuLinkChoice.SitePage;
            return;
        }

        if (item.Kind is MenuItemKind.Separator or MenuItemKind.Heading or MenuItemKind.EntityTarget)
        {
            _structuralKindText = item.Kind.ToString();
            _linkChoice = MenuLinkChoice.NoLink;
        }
        else
        {
            _linkChoice = InferLinkChoice(item.Kind, item.Url);
            ApplyLoadedUrl(item.Url);
        }

        if (item.CultureUrls == null)
        {
            return;
        }

        foreach (var pair in item.CultureUrls)
        {
            _cultureUrlRows.Add(new MenuCultureUrlRow { Culture = pair.Key, Url = pair.Value });
        }
    }

    private bool TryApplyLinkChoice()
    {
        _linkError = null;
        if (!string.IsNullOrEmpty(_structuralKindText)
            && Enum.TryParse<MenuItemKind>(_structuralKindText, out var structuralKind))
        {
            _model.Kind = structuralKind;
            if (structuralKind is MenuItemKind.Separator or MenuItemKind.Heading or MenuItemKind.EntityTarget)
            {
                _model.Url = null;
            }

            return structuralKind is MenuItemKind.Separator or MenuItemKind.Heading or MenuItemKind.EntityTarget
                || MenuItemUrlRules.IsAcceptable(_model.Kind, _model.Url);
        }

        switch (_linkChoice)
        {
            case MenuLinkChoice.SitePage:
                if (!MenuItemUrlRules.TryNormalizeSitePage(_pagePath, out var page))
                {
                    _linkError = L["MenuLink:PageInvalid"];
                    return false;
                }

                _model.Kind = MenuItemKind.InternalRoute;
                _model.Url = page;
                return true;
            case MenuLinkChoice.PageSection:
                if (!MenuItemUrlRules.TryNormalizeSection(_sectionIsCurrentPage, _sectionPagePath, _sectionId, out var section))
                {
                    _linkError = string.IsNullOrWhiteSpace(_sectionId) || !MenuItemUrlRules.IsSectionId(_sectionId.Trim())
                        ? L["MenuLink:SectionInvalid"]
                        : L["MenuLink:PageInvalid"];
                    return false;
                }

                _model.Kind = MenuItemKind.InternalRoute;
                _model.Url = section;
                return true;
            case MenuLinkChoice.Website:
                if (!MenuItemUrlRules.TryNormalizeWebsite(_websiteUrl, out var website))
                {
                    _linkError = L["MenuLink:WebsiteInvalid"];
                    return false;
                }

                _model.Kind = MenuItemKind.ExternalUrl;
                _model.Url = website;
                _websiteUrl = website;
                return true;
            case MenuLinkChoice.Email:
                if (!MenuItemUrlRules.TryNormalizeEmail(_email, out var email))
                {
                    _linkError = L["MenuLink:EmailInvalid"];
                    return false;
                }

                _model.Kind = MenuItemKind.ExternalUrl;
                _model.Url = email;
                return true;
            case MenuLinkChoice.Phone:
                if (!MenuItemUrlRules.TryNormalizePhone(_phone, out var phone))
                {
                    _linkError = L["MenuLink:PhoneInvalid"];
                    return false;
                }

                _model.Kind = MenuItemKind.ExternalUrl;
                _model.Url = phone;
                return true;
            default:
                _model.Kind = MenuItemKind.Container;
                _model.Url = null;
                return true;
        }
    }

    private bool TryApplyCultureUrls()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var valid = true;
        foreach (var row in _cultureUrlRows)
        {
            row.Error = null;
            if (string.IsNullOrWhiteSpace(row.Url))
            {
                continue;
            }

            var culture = MenuItemUrlRules.CanonicalCulture(row.Culture);
            var label = CultureLabel(row.Culture);
            if (culture == null || !seen.Add(culture))
            {
                row.Error = culture == null
                    ? L["MenuLink:CultureUrlInvalid", label]
                    : L["MenuLink:DuplicateCulture", label];
                valid = false;
                continue;
            }

            var url = MenuItemUrlRules.NormalizeCultureOverride(row.Url);
            if (url == null)
            {
                row.Error = L["MenuLink:CultureUrlInvalid", label];
                valid = false;
                continue;
            }

            row.Url = url;
            map[culture] = url;
        }

        if (valid)
        {
            _model.CultureUrls = map;
        }

        return valid;
    }

    private string CultureLabel(string? culture)
    {
        var match = _linkCultures.FirstOrDefault(item =>
            string.Equals(item.CultureName, culture, StringComparison.OrdinalIgnoreCase));
        return match == null || string.IsNullOrWhiteSpace(match.DisplayName) ? culture ?? string.Empty : match.DisplayName;
    }

    private void ApplyLoadedUrl(string? url)
    {
        switch (_linkChoice)
        {
            case MenuLinkChoice.SitePage:
                _pagePath = url ?? string.Empty;
                break;
            case MenuLinkChoice.PageSection:
                SplitSection(url, out _sectionIsCurrentPage, out _sectionPagePath, out _sectionId);
                break;
            case MenuLinkChoice.Website:
                _websiteUrl = url ?? string.Empty;
                break;
            case MenuLinkChoice.Email:
                _email = StripScheme(url, "mailto:");
                break;
            case MenuLinkChoice.Phone:
                _phone = StripScheme(url, "tel:");
                break;
        }
    }

    private static MenuLinkChoice InferLinkChoice(MenuItemKind kind, string? url)
    {
        if (kind == MenuItemKind.ExternalUrl)
        {
            if (url != null && url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                return MenuLinkChoice.Email;
            }

            if (url != null && url.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
            {
                return MenuLinkChoice.Phone;
            }

            return MenuLinkChoice.Website;
        }

        if (kind == MenuItemKind.InternalRoute)
        {
            return !string.IsNullOrEmpty(url) && url.Contains('#')
                ? MenuLinkChoice.PageSection
                : MenuLinkChoice.SitePage;
        }

        return MenuLinkChoice.NoLink;
    }

    private static void SplitSection(string? url, out bool currentPage, out string pagePath, out string sectionId)
    {
        currentPage = true;
        pagePath = string.Empty;
        sectionId = string.Empty;
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        var hash = url.IndexOf('#');
        if (hash < 0)
        {
            return;
        }

        if (MenuItemUrlRules.TryReadRootSection(url, out sectionId))
        {
            return;
        }

        sectionId = url.Substring(hash + 1);
        var path = url.Substring(0, hash);
        currentPage = false;
        pagePath = path;
    }

    private static string StripScheme(string? url, string scheme)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        return url.StartsWith(scheme, StringComparison.OrdinalIgnoreCase) ? url.Substring(scheme.Length) : url;
    }

    private static List<MenuLinkCulture> FallbackCultures() =>
        new()
        {
            new MenuLinkCulture { CultureName = "fa", DisplayName = "فارسی" },
            new MenuLinkCulture { CultureName = "en", DisplayName = "English" },
            new MenuLinkCulture { CultureName = "ar", DisplayName = "العربية" },
            new MenuLinkCulture { CultureName = "es", DisplayName = "Español" }
        };
}

public enum MenuLinkChoice
{
    SitePage,
    PageSection,
    Website,
    Email,
    Phone,
    NoLink
}

public sealed class MenuCultureUrlRow
{
    public string FieldId { get; } = "menu-culture-" + Guid.NewGuid().ToString("N");

    public string Culture { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? Error { get; set; }
}

public sealed class MenuLinkCulture
{
    public string CultureName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
}
