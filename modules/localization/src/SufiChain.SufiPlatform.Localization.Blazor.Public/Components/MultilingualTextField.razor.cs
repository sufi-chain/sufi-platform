using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Models;

namespace SufiChain.SufiPlatform.Localization.Blazor.Public.Components;

public partial class MultilingualTextField : ComponentBase
{
    private readonly Dictionary<string, string> _warnings = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _errors = new(StringComparer.OrdinalIgnoreCase);
    private ElementReference _defaultField;
    private bool _hasChanges;

    [Parameter] public string? Label { get; set; }

    [Parameter] public string? HelpText { get; set; }

    [Parameter] public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [Parameter] public EventCallback<Dictionary<string, string>> ValuesChanged { get; set; }

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

    public async Task<bool> ValidateAsync()
    {
        _errors.Clear();
        var rows = VisibleRows();
        var defaultRow = rows.FirstOrDefault(x => x.IsDefault) ?? rows.FirstOrDefault();
        if (Required && defaultRow != null)
        {
            var value = ValueOf(defaultRow.CultureName);
            if (MenuDisplayNamePlanner.IsBlankLabel(value))
            {
                _errors[defaultRow.CultureName] = MenuBusinessTexts.DefaultCultureRequiredMessage;
            }
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
                _errors[row.CultureName] = MenuBusinessTexts.LooksLikeKeyMessage;
            }
            else if (value.Trim().Length > MaxLength)
            {
                _errors[row.CultureName] = $"حداکثر {MaxLength} نویسه.";
            }
        }

        await InvokeAsync(StateHasChanged);
        if (_errors.Count > 0)
        {
            try
            {
                await _defaultField.FocusAsync();
            }
            catch (Exception)
            {
                // Focus is best-effort when the field is not interactive yet.
            }

            return false;
        }

        return true;
    }

    public Dictionary<string, string> GetValuesForSave()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var rows = AllRows();
        foreach (var row in rows)
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

    private string ValueOf(string culture)
    {
        if (Values != null && Values.TryGetValue(culture, out var value) && value != null)
        {
            return value;
        }

        return string.Empty;
    }

    private string PlaceholderFor(MultilingualCulture culture)
    {
        if (!string.IsNullOrWhiteSpace(ValueOf(culture.CultureName)) || culture.IsDefault)
        {
            return string.Empty;
        }

        var fallback = FallbackText;
        if (MenuDisplayNamePlanner.IsBlankLabel(fallback))
        {
            var defaultRow = AllRows().FirstOrDefault(x => x.IsDefault);
            fallback = defaultRow == null ? null : ValueOf(defaultRow.CultureName);
        }

        return MenuDisplayNamePlanner.IsBlankLabel(fallback)
            ? string.Empty
            : $"خالی بماند: «{fallback!.Trim()}» نمایش داده می‌شود";
    }

    private async Task OnValueChangedAsync(MultilingualCulture culture, ChangeEventArgs args)
    {
        Values ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var value = args.Value?.ToString() ?? string.Empty;
        Values[culture.CultureName] = value;
        _hasChanges = true;
        _warnings[culture.CultureName] = ScriptWarning(culture, value);
        if (_errors.ContainsKey(culture.CultureName) && !MenuDisplayNamePlanner.IsBlankLabel(value))
        {
            _errors.Remove(culture.CultureName);
        }

        await ValuesChanged.InvokeAsync(Values);
    }

    private static string ScriptWarning(MultilingualCulture culture, string value)
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
            return MenuBusinessTexts.ScriptWarningMessage;
        }

        if ((primary is "fa" or "ar") && latinOnly)
        {
            return MenuBusinessTexts.ScriptWarningMessage;
        }

        return string.Empty;
    }

    private string RowId(MultilingualCulture culture) =>
        $"mltf-{culture.CultureName}";
}
