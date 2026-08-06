namespace SufiChain.SufiPlatform.UI.ExceptionHandling;

/// <summary>
/// Context for exception information display.
/// </summary>
public class UserExceptionInformerContext
{
    /// <summary>
    /// The exception to inform the user about.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Optional custom title for the error message.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Optional custom message to display instead of exception message.
    /// </summary>
    public string? CustomMessage { get; set; }

    /// <summary>
    /// Whether to show exception details (stack trace, etc.).
    /// Defaults to false in production.
    /// </summary>
    public bool ShowDetails { get; set; }

    public UserExceptionInformerContext(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }
}
