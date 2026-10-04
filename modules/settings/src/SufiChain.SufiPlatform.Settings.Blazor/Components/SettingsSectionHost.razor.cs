using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.Settings.Blazor.Components;

/// <summary>
/// One settings group inside <c>SbSettingsLayout</c>.
/// Save and discard stay on this section. A group that cannot report a save result fails closed.
/// </summary>
public partial class SettingsSectionHost : ComponentBase
{
    private bool _dirty;
    private DynamicComponent? _component;
    private IEditableSettingGroup? _editor;
    private Dictionary<string, object?>? _parameters;

    [Parameter, EditorRequired]
    public SettingComponentGroup Group { get; set; } = default!;

    protected override void OnParametersSet()
    {
        _parameters = Group.Parameter == null
            ? null
            : new Dictionary<string, object?> { ["Parameter"] = Group.Parameter };
    }

    protected override void OnAfterRender(bool firstRender)
    {
        var editor = _component?.Instance as IEditableSettingGroup;
        if (!ReferenceEquals(editor, _editor))
        {
            if (_editor != null)
            {
                _editor.EditStateChanged -= OnEditorChanged;
            }

            _editor = editor;
            if (_editor != null)
            {
                _editor.EditStateChanged += OnEditorChanged;
            }
        }

        PublishDirty(_editor?.HasUnsavedChanges == true);
    }

    private void OnEditorChanged() => PublishDirty(_editor?.HasUnsavedChanges == true);

    private void PublishDirty(bool dirty)
    {
        if (dirty == _dirty)
        {
            return;
        }

        _dirty = dirty;
        _ = InvokeAsync(StateHasChanged);
    }

    private Task SetDirty(bool value)
    {
        _dirty = value;
        return Task.CompletedTask;
    }

    private async Task<bool> SaveAsync()
    {
        if (_component?.Instance is not IEditableSettingGroup editor)
        {
            return false;
        }

        var saved = await editor.TrySaveAsync();
        _dirty = editor.HasUnsavedChanges;
        return saved;
    }

    private async Task DiscardAsync()
    {
        if (_editor == null)
        {
            return;
        }

        await _editor.DiscardAsync();
        _dirty = _editor.HasUnsavedChanges;
    }
}
