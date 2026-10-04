using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Models;
using SufiChain.SufiPlatform.Localization.Localization;

namespace SufiChain.SufiPlatform.Localization.Blazor.Public.Components;

public partial class MultilingualTextField : ComponentBase
{
    private readonly Dictionary<string, string> _warnings = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _errors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, RowState> _rows = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _instanceId = Guid.NewGuid().ToString("N");
    private IJSObjectReference? _focusModule;
    private ValidationMessageStore? _validationMessages;
    private bool _hasChanges;

    [CascadingParameter] private EditContext? EditContext { get; set; }

    [Inject] private IStringLocalizer<SufiLocalizationResource> Text { get; set; } = default!;

    [Inject] private IJSRuntime Js { get; set; } = default!;

    [Parameter] public string? Label { get; set; }

    [Parameter] public string? HelpText { get; set; }

    [Parameter] public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [Parameter] public EventCallback<Dictionary<string, string>> ValuesChanged { get; set; }

    [Parameter] public IReadOnlyDictionary<string, string>? BaseValues { get; set; }

    [Parameter] public IReadOnlyList<MultilingualCulture>? Cultures { get; set; }

    [Parameter] public bool CulturesFailed { get; set; }

    [Parameter] public bool Required { get; set; } = true;

    [Parameter] public int MaxLength { get; set; } = 256;

    [Parameter] public bool Multiline { get; set; }

    [Parameter] public string? FallbackText { get; set; }

    [Parameter] public bool SameForAll { get; set; }

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public bool ShowBrokenBanner { get; set; }

    public bool HasChanges => _hasChanges;

    public IReadOnlyDictionary<string, string> CurrentValues => Values;

    protected override void OnParametersSet()
    {
        if (Values == null)
        {
            return;
        }

        foreach (var pair in Values)
        {
            if (string.IsNullOrWhiteSpace(pair.Key) || !_rows.TryGetValue(pair.Key, out var state))
            {
                continue;
            }

            var incoming = pair.Value ?? string.Empty;
            if (!string.Equals(state.Text, incoming, StringComparison.Ordinal))
            {
                state.Text = incoming;
            }
        }
    }

    public async Task<bool> ValidateAsync()
    {
        _errors.Clear();
        var rows = VisibleRows();
        var defaultRow = rows.FirstOrDefault(x => x.IsDefault) ?? rows.FirstOrDefault();
        if (Required && defaultRow != null && MenuDisplayNamePlanner.IsBlankLabel(ValueOf(defaultRow.CultureName)))
        {
            _errors[defaultRow.CultureName] = T("MultilingualTextField:Required", defaultRow.DisplayName);
        }

        foreach (var row in rows)
        {
            var value = ValueOf(row.CultureName);
            if (MenuDisplayNamePlanner.IsBlankLabel(value))
            {
                continue;
            }

            if (MenuDisplayNamePlanner.LooksLikeSystemKey(value))
            {
                _errors[row.CultureName] = T("MultilingualTextField:LooksLikeKey");
            }
            else if (value.Trim().Length > MaxLength)
            {
                _errors[row.CultureName] = T("MultilingualTextField:MaxLength", MaxLength);
            }
        }

        PublishErrors();
        await InvokeAsync(StateHasChanged);
        if (_errors.Count == 0)
        {
            return true;
        }

        var firstInvalid = rows.FirstOrDefault(row => _errors.ContainsKey(row.CultureName)) ?? defaultRow;
        if (firstInvalid != null)
        {
            await FocusRowAsync(RowId(firstInvalid));
        }

        return false;
    }

    public Dictionary<string, string> GetValuesForSave()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in AllRows())
        {
            var value = SameForAll && !row.IsDefault
                ? string.Empty
                : ValueOf(row.CultureName);
            result[row.CultureName] = MenuDisplayNamePlanner.NormalizeForCulture(row.CultureName, value);
        }

        return result;
    }

    private IReadOnlyList<MultilingualCulture> VisibleRows()
    {
        var rows = AllRows();
        if (!SameForAll)
        {
            return rows;
        }

        var first = rows.FirstOrDefault(x => x.IsDefault) ?? rows[0];
        return new[] { first };
    }

    private IReadOnlyList<MultilingualCulture> AllRows()
    {
        var source = Cultures?.Where(x => !string.IsNullOrWhiteSpace(x.CultureName)).ToList()
                     ?? new List<MultilingualCulture>();
        if (source.Count == 0)
        {
            source.Add(new MultilingualCulture
            {
                CultureName = "fa",
                DisplayName = "فارسی",
                IsRtl = true,
                IsDefault = true
            });
        }

        return source
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => source.IndexOf(x))
            .ToList();
    }

    private RowState StateFor(MultilingualCulture culture)
    {
        if (_rows.TryGetValue(culture.CultureName, out var state))
        {
            return state;
        }

        state = new RowState(culture.CultureName) { Text = ValueOf(culture.CultureName) };
        _rows[culture.CultureName] = state;
        return state;
    }

    private string ValueOf(string culture)
    {
        if (_rows.TryGetValue(culture, out var state))
        {
            return state.Text ?? string.Empty;
        }

        if (Values != null && Values.TryGetValue(culture, out var value) && value != null)
        {
            return value;
        }

        return string.Empty;
    }

    private string PlaceholderFor(MultilingualCulture culture)
    {
        if (!MenuDisplayNamePlanner.IsBlankLabel(ValueOf(culture.CultureName)))
        {
            return string.Empty;
        }

        var fallback = SuggestionText();
        if (MenuDisplayNamePlanner.IsBlankLabel(fallback))
        {
            if (culture.IsDefault)
            {
                return string.Empty;
            }

            var defaultRow = AllRows().FirstOrDefault(x => x.IsDefault);
            fallback = defaultRow == null ? null : ValueOf(defaultRow.CultureName);
        }

        if (MenuDisplayNamePlanner.IsBlankLabel(fallback) || MenuDisplayNamePlanner.LooksLikeSystemKey(fallback))
        {
            return string.Empty;
        }

        return culture.IsDefault
            ? fallback!.Trim()
            : T("MultilingualTextField:EmptyFallback", fallback!.Trim());
    }

    private string? SuggestionText()
    {
        if (MenuDisplayNamePlanner.IsBlankLabel(FallbackText) || MenuDisplayNamePlanner.LooksLikeSystemKey(FallbackText))
        {
            return null;
        }

        return FallbackText!.Trim();
    }

    private bool IsBase(MultilingualCulture culture)
    {
        if (BaseValues == null
            || !BaseValues.TryGetValue(culture.CultureName, out var baseValue)
            || MenuDisplayNamePlanner.IsBlankLabel(baseValue))
        {
            return false;
        }

        return string.Equals(ValueOf(culture.CultureName).Trim(), baseValue.Trim(), StringComparison.Ordinal);
    }

    private async Task OnValueChangedAsync(MultilingualCulture culture, RowState state, string? value)
    {
        var text = value ?? string.Empty;
        state.Text = text;
        Values ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Values[culture.CultureName] = text;
        _hasChanges = true;
        _warnings[culture.CultureName] = ScriptWarning(culture, text);
        if (_errors.ContainsKey(culture.CultureName) && !MenuDisplayNamePlanner.IsBlankLabel(text))
        {
            _errors.Remove(culture.CultureName);
        }

        await ValuesChanged.InvokeAsync(Values);
    }

    private string ScriptWarning(MultilingualCulture culture, string value)
    {
        if (MenuDisplayNamePlanner.IsBlankLabel(value))
        {
            return string.Empty;
        }

        var primary = MenuDisplayNamePlanner.PrimaryLanguage(culture.CultureName);
        var hasArabic = value.Any(ch => ch is >= '\u0600' and <= '\u06FF');
        var letters = value.Where(char.IsLetter).ToList();
        var latinOnly = letters.Count > 0 && letters.All(ch => ch <= '\u024F');
        if ((primary is "en" or "es") && hasArabic)
        {
            return T("MultilingualTextField:ScriptWarning");
        }

        if ((primary is "fa" or "ar") && latinOnly)
        {
            return T("MultilingualTextField:ScriptWarning");
        }

        return string.Empty;
    }

    private Dictionary<string, object> RowAttributes(MultilingualCulture culture) =>
        new()
        {
            ["dir"] = culture.IsRtl ? "rtl" : "ltr",
            ["lang"] = culture.CultureName,
            ["maxlength"] = MaxLength.ToString()
        };

    private void PublishErrors()
    {
        if (EditContext == null)
        {
            return;
        }

        _validationMessages ??= new ValidationMessageStore(EditContext);
        _validationMessages.Clear();
        foreach (var row in AllRows())
        {
            if (!_errors.TryGetValue(row.CultureName, out var error))
            {
                continue;
            }

            _validationMessages.Add(new FieldIdentifier(StateFor(row), nameof(RowState.Text)), error);
        }

        EditContext.NotifyValidationStateChanged();
    }

    private string RowId(MultilingualCulture culture) =>
        $"mltf-{_instanceId}-{culture.CultureName}";

    private string ErrorId(MultilingualCulture culture) =>
        $"{RowId(culture)}-error";

    private string T(string name) => Text[name].Value;

    private string T(string name, params object[] arguments) => Text[name, arguments].Value;

    private async Task FocusRowAsync(string elementId)
    {
        try
        {
            _focusModule ??= await Js.InvokeAsync<IJSObjectReference>(
                "import",
                "./_content/SufiChain.SufiPlatform.Localization.Blazor.Public/multilingual-text-field.js");
            await _focusModule.InvokeVoidAsync("focus", elementId);
        }
        catch (Exception)
        {
            // Focus is best-effort when the field is not interactive yet.
        }
    }

    private sealed class RowState
    {
        public RowState(string cultureName)
        {
            CultureName = cultureName;
        }

        public string CultureName { get; }

        public string Text { get; set; } = string.Empty;
    }
}
