using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Volo.Abp.Authorization;

namespace SufiChain.SufiPlatform.UI.Blazor.Components;

/// <summary>
/// Platform settings section. <see cref="Policy"/> shows the section, and <see cref="ReadOnlyPolicy"/> shows it without Save.
/// A caller who has neither policy does not see the section.
/// </summary>
public partial class SufiSettingsSection : ComponentBase
{
    private bool _policyResolved;
    private bool _policyVisible;
    private bool _policyReadOnly;

    [Inject]
    protected IAuthorizationService AuthorizationService { get; set; } = default!;

    /// <summary>
    /// Permission that grants an editable section. Without it, <see cref="ReadOnlyPolicy"/> can still show a view-only section.
    /// </summary>
    [Parameter]
    public string? Policy { get; set; }

    /// <summary>
    /// Permission that grants a view-only section when <see cref="Policy"/> is not granted.
    /// </summary>
    [Parameter]
    public string? ReadOnlyPolicy { get; set; }

    /// <summary>
    /// Stable kebab-case id. Never localized.
    /// </summary>
    [Parameter, EditorRequired]
    public string Id { get; set; } = "";

    /// <summary>
    /// Rail label.
    /// </summary>
    [Parameter, EditorRequired]
    public string Label { get; set; } = "";

    /// <summary>
    /// Registered SufiIcons name.
    /// </summary>
    [Parameter, EditorRequired]
    public string Icon { get; set; } = "";

    /// <summary>
    /// Save-bar heading. Falls back to <see cref="Label"/>.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// One line under the save-bar heading.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Extra visibility flag. A policy denial also hides the section.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Forces view-only even when <see cref="Policy"/> is granted.
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Feature unavailable. Not used for permissions.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Why the section is disabled.
    /// </summary>
    [Parameter]
    public string? DisabledReason { get; set; }

    /// <summary>
    /// Optional rail badge.
    /// </summary>
    [Parameter]
    public RenderFragment? Badge { get; set; }

    /// <summary>
    /// Unsaved changes reported by the section body.
    /// </summary>
    [Parameter]
    public bool IsDirty { get; set; }

    /// <summary>
    /// Raised when the save bar clears the dirty flag.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsDirtyChanged { get; set; }

    /// <summary>
    /// Saves the section. Null means there is no form to save.
    /// </summary>
    [Parameter]
    public Func<Task<bool>>? OnSave { get; set; }

    /// <summary>
    /// Restores the last saved values.
    /// </summary>
    [Parameter]
    public Func<Task>? OnDiscard { get; set; }

    /// <summary>
    /// False while the form is invalid.
    /// </summary>
    [Parameter]
    public bool CanSave { get; set; } = true;

    /// <summary>
    /// Secondary save-bar actions.
    /// </summary>
    [Parameter]
    public RenderFragment? BarActions { get; set; }

    /// <summary>
    /// Content renders on first activation and stays mounted.
    /// </summary>
    [Parameter]
    public bool Lazy { get; set; } = true;

    /// <summary>
    /// Section body.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private bool HasPolicy => !string.IsNullOrWhiteSpace(Policy) || !string.IsNullOrWhiteSpace(ReadOnlyPolicy);

    private bool EffectiveVisible => Visible && (!HasPolicy || (_policyResolved && _policyVisible));

    private bool EffectiveReadOnly => ReadOnly || _policyReadOnly;

    protected override async Task OnParametersSetAsync()
    {
        if (!HasPolicy)
        {
            _policyResolved = true;
            _policyVisible = true;
            _policyReadOnly = false;
            return;
        }

        var canEdit = !string.IsNullOrWhiteSpace(Policy) && await AuthorizationService.IsGrantedAsync(Policy);
        var canView = !string.IsNullOrWhiteSpace(ReadOnlyPolicy) && await AuthorizationService.IsGrantedAsync(ReadOnlyPolicy);

        if (canEdit)
        {
            _policyVisible = true;
            _policyReadOnly = false;
        }
        else if (canView || (string.IsNullOrWhiteSpace(Policy) && canView))
        {
            _policyVisible = true;
            _policyReadOnly = true;
        }
        else if (string.IsNullOrWhiteSpace(Policy) && string.IsNullOrWhiteSpace(ReadOnlyPolicy))
        {
            _policyVisible = true;
            _policyReadOnly = false;
        }
        else
        {
            _policyVisible = false;
            _policyReadOnly = false;
        }

        _policyResolved = true;
    }
}
