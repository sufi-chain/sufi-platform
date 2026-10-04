using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.Menus.Menus;

namespace SufiChain.SufiPlatform.Menus.Blazor.Pages;

public partial class MenuItemEditor
{
    private MenuLinkChoice _linkChoice = MenuLinkChoice.SitePage;
    private string _structuralKindText = string.Empty;
    private string _pagePath = string.Empty;
    private MenuSectionPage _sectionPage = MenuSectionPage.CurrentPage;
    private string _sectionPagePath = string.Empty;
    private string _sectionId = string.Empty;
    private string _websiteUrl = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string? _linkError;
    private string? _nameError;
    private string? _displayOrderError;
    private string? _targetIdError;
    private readonly List<MenuEditorError> _errorFields = new();
    private string? _focusFieldId;
    private bool _openAdvancedOnRender;
    private bool _linkTargetTouched;
    private string? _linkErrorFieldId;
    private string? _linkErrorLabel;
    private object? _linkErrorModel;
    private string? _linkErrorMember;
    private ElementReference _advancedRef;
    private EditContext? _submittedContext;
    private ValidationMessageStore? _validationMessages;
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
                // The summary still names the errors when a panel cannot be opened from script.
            }
        }

        if (string.IsNullOrEmpty(_focusFieldId))
        {
            return;
        }

        var fieldId = _focusFieldId;
        _focusFieldId = null;
        try
        {
            _editorModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./_content/SufiChain.SufiPlatform.Menus.Blazor/menu-item-editor.js");
            await _editorModule.InvokeVoidAsync("focusField", fieldId);
        }
        catch (Exception)
        {
            // The summary still names the field when focus is not available.
        }
    }

    private void PublishValidation(bool displayNameFailed)
    {
        _errorFields.Clear();
        if (displayNameFailed)
        {
            _errorFields.Add(new MenuEditorError(L["DisplayName"].ToString(), null));
        }

        if (!string.IsNullOrEmpty(_nameError))
        {
            _errorFields.Add(new MenuEditorError(L["MenuName"].ToString(), "menu-item-name"));
        }

        if (!string.IsNullOrEmpty(_linkError))
        {
            _errorFields.Add(new MenuEditorError(_linkErrorLabel ?? L["MenuItemSection:Link"].ToString(), _linkErrorFieldId));
        }

        if (!string.IsNullOrEmpty(_displayOrderError))
        {
            _errorFields.Add(new MenuEditorError(L["DisplayOrder"].ToString(), "menu-item-order"));
        }

        if (!string.IsNullOrEmpty(_targetIdError))
        {
            _errorFields.Add(new MenuEditorError(L["TargetId"].ToString(), "menu-item-target-id"));
        }

        foreach (var row in _cultureUrlRows.Where(row => !string.IsNullOrEmpty(row.Error)))
        {
            _errorFields.Add(new MenuEditorError(CultureLabel(row.Culture), row.FieldId));
        }

        _openAdvancedOnRender = HasAdvancedErrors() || !string.IsNullOrEmpty(_nameError);
        _focusFieldId = displayNameFailed
            ? null
            : _errorFields.Select(field => field.FieldId).FirstOrDefault(id => !string.IsNullOrEmpty(id));
        ApplyAccessibilityState();
    }

    private void ApplyAccessibilityState()
    {
        if (_submittedContext == null)
        {
            return;
        }

        _validationMessages ??= new ValidationMessageStore(_submittedContext);
        _validationMessages.Clear();
        if (!string.IsNullOrEmpty(_nameError))
        {
            _validationMessages.Add(new FieldIdentifier(_model, nameof(CreateMenuItemDto.Name)), _nameError);
        }

        if (!string.IsNullOrEmpty(_linkError) && _linkErrorModel != null && !string.IsNullOrEmpty(_linkErrorMember))
        {
            _validationMessages.Add(new FieldIdentifier(_linkErrorModel, _linkErrorMember), _linkError);
        }

        if (!string.IsNullOrEmpty(_displayOrderError))
        {
            _validationMessages.Add(new FieldIdentifier(this, nameof(_displayOrderText)), _displayOrderError);
        }

        if (!string.IsNullOrEmpty(_targetIdError))
        {
            _validationMessages.Add(new FieldIdentifier(this, nameof(_targetIdText)), _targetIdError);
        }

        foreach (var row in _cultureUrlRows.Where(row => !string.IsNullOrEmpty(row.Error)))
        {
            _validationMessages.Add(new FieldIdentifier(row, nameof(MenuCultureUrlRow.Url)), row.Error!);
        }

        _submittedContext.NotifyValidationStateChanged();
    }

    private string? DescribedBy(string fieldId, string? error) =>
        string.IsNullOrEmpty(error) ? null : fieldId + "-error";

    private bool HasAdvancedErrors() =>
        !string.IsNullOrEmpty(_targetIdError) || _cultureUrlRows.Any(row => !string.IsNullOrEmpty(row.Error));

    private void OnLinkChoiceChanged(MenuLinkChoice choice)
    {
        var previous = _linkChoice;
        _linkChoice = choice;
        _structuralKindText = string.Empty;
        _linkError = null;
        if (choice == MenuLinkChoice.Website && previous != MenuLinkChoice.Website && !_linkTargetTouched)
        {
            _model.LinkTarget = MenuLinkTarget.NewTab;
        }
    }

    private void OnLinkTargetChanged(MenuLinkTarget target)
    {
        _linkTargetTouched = true;
        _model.LinkTarget = target;
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
        _errorFields.Clear();
        _structuralKindText = string.Empty;
        _pagePath = string.Empty;
        _sectionPage = MenuSectionPage.CurrentPage;
        _linkTargetTouched = item != null;
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
        _linkErrorFieldId = null;
        _linkErrorLabel = null;
        _linkErrorModel = null;
        _linkErrorMember = null;
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
                    SetLinkFieldError("menu-item-page-path", L["MenuLink:PagePath"], L["MenuLink:PageInvalid"], this, nameof(_pagePath));
                    return false;
                }

                _model.Kind = MenuItemKind.InternalRoute;
                _model.Url = page;
                return true;
            case MenuLinkChoice.PageSection:
                if (!MenuItemUrlRules.TryNormalizeSection(_sectionPage, _sectionPagePath, _sectionId, out var section))
                {
                    if (string.IsNullOrWhiteSpace(_sectionId) || !MenuItemUrlRules.IsSectionId(_sectionId.Trim()))
                    {
                        SetLinkFieldError("menu-item-section-id", L["MenuLink:SectionId"], L["MenuLink:SectionInvalid"], this, nameof(_sectionId));
                    }
                    else
                    {
                        SetLinkFieldError("menu-item-section-path", L["MenuLink:PagePath"], L["MenuLink:PageInvalid"], this, nameof(_sectionPagePath));
                    }

                    return false;
                }

                _model.Kind = MenuItemKind.InternalRoute;
                _model.Url = section;
                return true;
            case MenuLinkChoice.Website:
                if (!MenuItemUrlRules.TryNormalizeWebsite(_websiteUrl, out var website))
                {
                    SetLinkFieldError("menu-item-website", L["MenuLink:WebsiteUrl"], L["MenuLink:WebsiteInvalid"], this, nameof(_websiteUrl));
                    return false;
                }

                _model.Kind = MenuItemKind.ExternalUrl;
                _model.Url = website;
                _websiteUrl = website;
                return true;
            case MenuLinkChoice.Email:
                if (!MenuItemUrlRules.TryNormalizeEmail(_email, out var email))
                {
                    SetLinkFieldError("menu-item-email", L["MenuLink:EmailAddress"], L["MenuLink:EmailInvalid"], this, nameof(_email));
                    return false;
                }

                _model.Kind = MenuItemKind.ExternalUrl;
                _model.Url = email;
                return true;
            case MenuLinkChoice.Phone:
                if (!MenuItemUrlRules.TryNormalizePhone(_phone, out var phone))
                {
                    SetLinkFieldError("menu-item-phone", L["MenuLink:PhoneNumber"], L["MenuLink:PhoneInvalid"], this, nameof(_phone));
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
                SplitSection(url, out _sectionPage, out _sectionPagePath, out _sectionId);
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

    private void SetLinkFieldError(string fieldId, string label, string message, object model, string member)
    {
        _linkError = message;
        _linkErrorFieldId = fieldId;
        _linkErrorLabel = label;
        _linkErrorModel = model;
        _linkErrorMember = member;
    }

    private static void SplitSection(string? url, out MenuSectionPage page, out string pagePath, out string sectionId)
    {
        page = MenuSectionPage.CurrentPage;
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

        sectionId = url.Substring(hash + 1);
        if (url.StartsWith("/#", StringComparison.Ordinal))
        {
            page = MenuSectionPage.HomePage;
            return;
        }

        if (url[0] == '#')
        {
            page = MenuSectionPage.CurrentPage;
            return;
        }

        page = MenuSectionPage.OtherPage;
        pagePath = url.Substring(0, hash);
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

public sealed class MenuEditorError
{
    public MenuEditorError(string label, string? fieldId)
    {
        Label = label;
        FieldId = fieldId;
    }

    public string Label { get; }

    public string? FieldId { get; }
}
