using System.Text.Json;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

/// <summary>
/// Compares a settings model with the last loaded or saved copy.
/// </summary>
public sealed class SettingGroupEditor<T> where T : class
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Func<T?> _read;
    private readonly Action<T?> _write;
    private string? _baseline;

    public SettingGroupEditor(Func<T?> read, Action<T?> write)
    {
        _read = read;
        _write = write;
    }

    public bool HasUnsavedChanges { get; private set; }

    public event Action? Changed;

    public void Capture()
    {
        _baseline = JsonSerializer.Serialize(_read(), JsonOptions);
        SetDirty(false);
    }

    public void Observe()
    {
        if (_baseline == null)
        {
            return;
        }

        var current = JsonSerializer.Serialize(_read(), JsonOptions);
        SetDirty(!string.Equals(current, _baseline, StringComparison.Ordinal));
    }

    public void Restore()
    {
        if (_baseline == null)
        {
            return;
        }

        _write(JsonSerializer.Deserialize<T>(_baseline, JsonOptions));
        SetDirty(false);
    }

    private void SetDirty(bool dirty)
    {
        if (HasUnsavedChanges == dirty)
        {
            return;
        }

        HasUnsavedChanges = dirty;
        Changed?.Invoke();
    }
}
