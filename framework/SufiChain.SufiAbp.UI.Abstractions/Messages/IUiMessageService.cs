using Microsoft.Extensions.Localization;

namespace SufiChain.SufiAbp.UI.Messages;

/// <summary>
/// Service for displaying modal dialogs and confirmations to users.
/// </summary>
public interface IUiMessageService
{
    /// <summary>
    /// Shows a confirmation dialog and returns true if confirmed.
    /// </summary>
    Task<bool> ConfirmAsync(string message, string? title = null, Action<UiMessageOptions>? options = null);

    /// <summary>
    /// Shows an info message dialog.
    /// </summary>
    Task InfoAsync(string message, string? title = null, Action<UiMessageOptions>? options = null);

    /// <summary>
    /// Shows a warning message dialog.
    /// </summary>
    Task WarnAsync(string message, string? title = null, Action<UiMessageOptions>? options = null);

    /// <summary>
    /// Shows an error message dialog.
    /// </summary>
    Task ErrorAsync(string message, string? title = null, Action<UiMessageOptions>? options = null);

    /// <summary>
    /// Shows a success message dialog.
    /// </summary>
    Task SuccessAsync(string message, string? title = null, Action<UiMessageOptions>? options = null);
    Task Success(LocalizedString localizedString);
}
