using System;

namespace SufiChain.SufiPlatform.FileManager.Uploads;

/// <summary>
/// The structure an uploader sends with each file (sufi-chain#479). The File Manager keeps its uploader mounted
/// and changes the StructureKey parameter as the user moves between folders. The key used to be read only on
/// first render, so a page that opened at the root sent General for every later upload. Files dropped into
/// «پیوست‌های هوش مصنوعی» or «پیوست‌های گفت‌وگو» were stored (200) but never listed, since a folder lists only
/// files of its own structure.
/// </summary>
public sealed class UploaderStructureKeyState
{
    private string? _parameterKey;
    private bool _initialized;

    public string Selected { get; private set; } = FileStructureKeys.General;

    /// <summary>Applies the parent's parameter. Returns true when the selected structure changed.</summary>
    public bool ApplyParameter(string? parameterKey)
    {
        if (_initialized && string.Equals(parameterKey, _parameterKey, StringComparison.Ordinal))
        {
            // Same parameter on a re-render: keep a structure the user picked in the selector.
            return false;
        }

        _initialized = true;
        _parameterKey = parameterKey;
        var next = string.IsNullOrWhiteSpace(parameterKey) ? FileStructureKeys.General : parameterKey;
        var changed = !string.Equals(next, Selected, StringComparison.Ordinal);
        Selected = next;
        return changed;
    }

    /// <summary>The user picked a structure in the uploader's own selector.</summary>
    public void Select(string? structureKey)
    {
        Selected = string.IsNullOrWhiteSpace(structureKey) ? FileStructureKeys.General : structureKey;
    }
}
