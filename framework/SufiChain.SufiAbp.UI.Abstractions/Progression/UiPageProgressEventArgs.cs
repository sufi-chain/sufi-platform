namespace SufiChain.SufiAbp.UI.Progression;

/// <summary>
/// Event arguments for page progress changes.
/// </summary>
public class UiPageProgressEventArgs : EventArgs
{
    /// <summary>
    /// The progress percentage (0-100), or null for indeterminate/hidden.
    /// </summary>
    public int? Percentage { get; }

    /// <summary>
    /// The visual type of the progress bar.
    /// </summary>
    public UiPageProgressType Type { get; }

    /// <summary>
    /// Whether the progress is visible.
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    /// Whether the progress is indeterminate (unknown completion).
    /// </summary>
    public bool IsIndeterminate { get; }

    public UiPageProgressEventArgs(int? percentage, UiPageProgressType type, bool isVisible, bool isIndeterminate = false)
    {
        Percentage = percentage;
        Type = type;
        IsVisible = isVisible;
        IsIndeterminate = isIndeterminate;
    }
}
