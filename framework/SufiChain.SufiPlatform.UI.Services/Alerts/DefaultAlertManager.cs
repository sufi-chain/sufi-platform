using SufiChain.SufiPlatform.UI.Alerts;

namespace SufiChain.SufiPlatform.UI.Services.Alerts;

/// <summary>
/// Default implementation of IAlertManager with event notifications.
/// </summary>
public class DefaultAlertManager : IAlertManager
{
    private readonly List<AlertInfo> _alerts = new();
    private readonly object _alertsLock = new();

    /// <summary>
    /// Event raised when alerts change.
    /// </summary>
    public event Action? OnAlertsChanged;

    /// <inheritdoc/>
    public IReadOnlyList<AlertInfo> Alerts
    {
        get
        {
            lock (_alertsLock)
            {
                return _alerts.ToList().AsReadOnly();
            }
        }
    }

    /// <inheritdoc/>
    public void AddAlert(AlertType type, string message, string? title = null, bool dismissible = true)
    {
        lock (_alertsLock)
        {
            _alerts.Add(new AlertInfo(type, message, title, dismissible));
        }
        OnAlertsChanged?.Invoke();
    }

    /// <inheritdoc/>
    public void Info(string message, string? title = null, bool dismissible = true)
    {
        AddAlert(AlertType.Info, message, title, dismissible);
    }

    /// <inheritdoc/>
    public void Success(string message, string? title = null, bool dismissible = true)
    {
        AddAlert(AlertType.Success, message, title, dismissible);
    }

    /// <inheritdoc/>
    public void Warning(string message, string? title = null, bool dismissible = true)
    {
        AddAlert(AlertType.Warning, message, title, dismissible);
    }

    /// <inheritdoc/>
    public void Danger(string message, string? title = null, bool dismissible = true)
    {
        AddAlert(AlertType.Danger, message, title, dismissible);
    }

    /// <summary>
    /// Removes a specific alert.
    /// </summary>
    public void RemoveAlert(AlertInfo alert)
    {
        bool removed;
        lock (_alertsLock)
        {
            removed = _alerts.Remove(alert);
        }
        if (removed)
        {
            OnAlertsChanged?.Invoke();
        }
    }

    /// <inheritdoc/>
    public void ClearAlerts()
    {
        lock (_alertsLock)
        {
            _alerts.Clear();
        }
        OnAlertsChanged?.Invoke();
    }
}
