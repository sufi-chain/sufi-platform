using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
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
    private string? _resolvedPolicy;
    private string? _resolvedReadOnlyPolicy;
    private int _resolveVersion;

    [Inject]
    protected IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    protected ILogger<SufiSettingsSection> Logger { get; set; } = default!;

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

    protected override void OnParametersSet()
    {
        if (!HasPolicy)
        {
            _policyResolved = true;
            _policyVisible = true;
            _policyReadOnly = false;
            _resolvedPolicy = Policy;
            _resolvedReadOnlyPolicy = ReadOnlyPolicy;
            return;
        }

        if (_policyResolved
            && string.Equals(_resolvedPolicy, Policy, StringComparison.Ordinal)
            && string.Equals(_resolvedReadOnlyPolicy, ReadOnlyPolicy, StringComparison.Ordinal))
        {
            return;
        }

        _policyResolved = false;
        var version = ++_resolveVersion;
        _ = ResolvePolicyAsync(version, Policy, ReadOnlyPolicy);
    }

    private async Task ResolvePolicyAsync(int version, string? policy, string? readOnlyPolicy)
    {
        var canEdit = false;
        var canView = false;
        try
        {
            canEdit = !string.IsNullOrWhiteSpace(policy) && await AuthorizationService.IsGrantedAsync(policy);
            if (!canEdit && !string.IsNullOrWhiteSpace(readOnlyPolicy))
            {
                canView = await AuthorizationService.IsGrantedAsync(readOnlyPolicy);
            }
        }
        catch (Exception exception)
        {
            if (version != _resolveVersion)
            {
                return;
            }

            Logger.LogWarning(exception, "Settings section {SectionId} permission check failed.", Id);
            canEdit = false;
            canView = false;
        }

        if (version != _resolveVersion)
        {
            return;
        }

        if (canEdit)
        {
            _policyVisible = true;
            _policyReadOnly = false;
        }
        else if (canView)
        {
            _policyVisible = true;
            _policyReadOnly = true;
        }
        else
        {
            _policyVisible = false;
            _policyReadOnly = false;
        }

        _resolvedPolicy = policy;
        _resolvedReadOnlyPolicy = readOnlyPolicy;
        _policyResolved = true;
        await InvokeAsync(StateHasChanged);
    }
}
