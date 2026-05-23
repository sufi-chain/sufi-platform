namespace SufiChain.SufiAbp.UI.Alerts;

/// <summary>
/// Types of alerts that can be displayed.
/// </summary>
public enum AlertType
{
    /// <summary>
    /// Informational alert.
    /// </summary>
    Info,

    /// <summary>
    /// Success alert.
    /// </summary>
    Success,

    /// <summary>
    /// Warning alert.
    /// </summary>
    Warning,

    /// <summary>
    /// Error/danger alert.
    /// </summary>
    Danger,

    /// <summary>
    /// Default/primary alert.
    /// </summary>
    Default
}
