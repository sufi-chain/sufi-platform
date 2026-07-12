namespace SufiChain.SufiPlatform.UI.Notifications;

/// <summary>
/// Options for UI notifications.
/// </summary>
public class UiNotificationOptions
{
    /// <summary>
    /// Duration in milliseconds before the notification auto-dismisses.
    /// Null means use default duration.
    /// </summary>
    public int? DurationMs { get; set; }

    /// <summary>
    /// Whether the notification can be dismissed by the user.
    /// </summary>
    public bool Dismissible { get; set; } = true;

    /// <summary>
    /// Position of the notification on screen.
    /// </summary>
    public UiNotificationPosition Position { get; set; } = UiNotificationPosition.TopRight;
}

/// <summary>
/// Position of notifications on the screen.
/// </summary>
public enum UiNotificationPosition
{
    /// <summary>
    /// Top right corner.
    /// </summary>
    TopRight,

    /// <summary>
    /// Top left corner.
    /// </summary>
    TopLeft,

    /// <summary>
    /// Top center.
    /// </summary>
    TopCenter,

    /// <summary>
    /// Bottom right corner.
    /// </summary>
    BottomRight,

    /// <summary>
    /// Bottom left corner.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// Bottom center.
    /// </summary>
    BottomCenter
}
