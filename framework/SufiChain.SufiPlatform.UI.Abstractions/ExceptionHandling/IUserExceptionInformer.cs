namespace SufiChain.SufiPlatform.UI.ExceptionHandling;

/// <summary>
/// Service for informing users about exceptions.
/// </summary>
public interface IUserExceptionInformer
{
    /// <summary>
    /// Informs the user about an exception synchronously.
    /// </summary>
    void Inform(UserExceptionInformerContext context);

    /// <summary>
    /// Informs the user about an exception asynchronously.
    /// </summary>
    Task InformAsync(UserExceptionInformerContext context);

    /// <summary>
    /// Builds the localized user-facing message for an exception without presenting a dialog.
    /// Use this for inline banners; prefer <see cref="InformAsync"/> / HandleErrorAsync for dialogs.
    /// </summary>
    string GetLocalizedMessage(Exception exception);
}
