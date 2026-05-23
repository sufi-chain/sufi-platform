namespace SufiChain.SufiAbp.UI.Progression;

/// <summary>
/// Service for managing page progress indicators.
/// </summary>
public interface IUiPageProgressService
{
    /// <summary>
    /// Event raised when progress changes.
    /// </summary>
    event EventHandler<UiPageProgressEventArgs>? ProgressChanged;

    /// <summary>
    /// Sets the progress value.
    /// </summary>
    /// <param name="percentage">Progress percentage (0-100), or null to hide/complete.</param>
    /// <param name="type">The visual type/color of the progress bar.</param>
    Task SetProgressAsync(int? percentage, UiPageProgressType type = UiPageProgressType.Default);

    /// <summary>
    /// Shows indeterminate progress.
    /// </summary>
    Task ShowIndeterminateAsync(UiPageProgressType type = UiPageProgressType.Default);

    /// <summary>
    /// Hides the progress indicator.
    /// </summary>
    Task HideAsync();
}
