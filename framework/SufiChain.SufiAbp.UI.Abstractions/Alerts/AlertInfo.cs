namespace SufiChain.SufiAbp.UI.Alerts;

/// <summary>
/// Information about an alert to be displayed.
/// </summary>
public class AlertInfo
{
    /// <summary>
    /// The type of alert.
    /// </summary>
    public AlertType Type { get; set; }

    /// <summary>
    /// The alert message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Optional title for the alert.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Whether the alert can be dismissed by the user.
    /// </summary>
    public bool Dismissible { get; set; } = true;

    /// <summary>
    /// Creates a new AlertInfo.
    /// </summary>
    /// <param name="type">The alert type.</param>
    /// <param name="message">The alert message.</param>
    /// <param name="title">Optional title.</param>
    /// <param name="dismissible">Whether dismissible (default: true).</param>
    public AlertInfo(AlertType type, string message, string? title = null, bool dismissible = true)
    {
        Type = type;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Title = title;
        Dismissible = dismissible;
    }
}
