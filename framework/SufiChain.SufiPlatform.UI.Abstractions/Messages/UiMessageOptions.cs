namespace SufiChain.SufiPlatform.UI.Messages;

/// <summary>
/// Options for UI message dialogs.
/// </summary>
public class UiMessageOptions
{
    /// <summary>
    /// Whether to center the message text.
    /// </summary>
    public bool CenterMessage { get; set; }

    /// <summary>
    /// Whether to show an icon for the message type.
    /// </summary>
    public bool ShowMessageIcon { get; set; } = true;

    /// <summary>
    /// Custom text for the OK button.
    /// </summary>
    public string? OkButtonText { get; set; }

    /// <summary>
    /// Custom text for the confirm button in confirmation dialogs.
    /// </summary>
    public string? ConfirmButtonText { get; set; }

    /// <summary>
    /// Custom text for the cancel button in confirmation dialogs.
    /// </summary>
    public string? CancelButtonText { get; set; }

    /// <summary>
    /// Whether the dialog can be closed by clicking outside or pressing escape.
    /// </summary>
    public bool CloseOnBackdropClick { get; set; } = false;
}
