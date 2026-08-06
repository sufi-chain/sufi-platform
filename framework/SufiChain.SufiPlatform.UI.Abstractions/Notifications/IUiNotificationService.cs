namespace SufiChain.SufiPlatform.UI.Notifications;

/// <summary>
/// Service for displaying toast notifications to users.
/// </summary>
public interface IUiNotificationService
{
    /// <summary>
    /// Shows an info notification.
    /// </summary>
    Task InfoAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null);

    /// <summary>
    /// Shows a warning notification.
    /// </summary>
    Task WarnAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null);

    /// <summary>
    /// Shows an error notification.
    /// </summary>
    Task ErrorAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null);

    /// <summary>
    /// Shows a success notification.
    /// </summary>
    Task SuccessAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null);
}
