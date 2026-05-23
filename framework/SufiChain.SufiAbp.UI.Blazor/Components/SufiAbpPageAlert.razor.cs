using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.UI.Alerts;
using SufiChain.SufiAbp.UI.Services.Alerts;
using SufiChain.SufiBlazor.Components.Feedback;

namespace SufiChain.SufiAbp.UI.Blazor.Components;

/// <summary>
/// Component for displaying page-level alerts using SufiBlazor Alert component.
/// </summary>
public partial class SufiAbpPageAlert : ComponentBase, IDisposable
{
    [Inject]
    protected IAlertManager AlertManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        if (AlertManager is DefaultAlertManager defaultManager)
        {
            defaultManager.OnAlertsChanged += OnAlertsChanged;
        }
    }

    private void OnAlertsChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private SbAlertSeverity GetAlertSeverity(AlertType type) => type switch
    {
        AlertType.Success => SbAlertSeverity.Success,
        AlertType.Warning => SbAlertSeverity.Warning,
        AlertType.Danger => SbAlertSeverity.Danger,
        AlertType.Info => SbAlertSeverity.Info,
        _ => SbAlertSeverity.Info
    };

    private void DismissAlert(AlertInfo alert)
    {
        if (AlertManager is DefaultAlertManager defaultManager)
        {
            defaultManager.RemoveAlert(alert);
        }
        else
        {
            // Fallback: clear all alerts
            AlertManager.ClearAlerts();
        }
    }

    public void Dispose()
    {
        if (AlertManager is DefaultAlertManager defaultManager)
        {
            defaultManager.OnAlertsChanged -= OnAlertsChanged;
        }
    }
}
