namespace SufiChain.SufiPlatform.UI.Alerts;

/// <summary>
/// Manages page alerts.
/// </summary>
public interface IAlertManager
{
    /// <summary>
    /// Gets the current alerts.
    /// </summary>
    IReadOnlyList<AlertInfo> Alerts { get; }

    /// <summary>
    /// Adds an alert.
    /// </summary>
    /// <param name="type">The alert type.</param>
    /// <param name="message">The alert message.</param>
    /// <param name="title">Optional title.</param>
    /// <param name="dismissible">Whether dismissible (default: true).</param>
    void AddAlert(AlertType type, string message, string? title = null, bool dismissible = true);

    /// <summary>
    /// Adds an info alert.
    /// </summary>
    void Info(string message, string? title = null, bool dismissible = true);

    /// <summary>
    /// Adds a success alert.
    /// </summary>
    void Success(string message, string? title = null, bool dismissible = true);

    /// <summary>
    /// Adds a warning alert.
    /// </summary>
    void Warning(string message, string? title = null, bool dismissible = true);

    /// <summary>
    /// Adds a danger/error alert.
    /// </summary>
    void Danger(string message, string? title = null, bool dismissible = true);

    /// <summary>
    /// Clears all alerts.
    /// </summary>
    void ClearAlerts();
}
