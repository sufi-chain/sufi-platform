namespace SufiChain.SufiAbp.UI.ExceptionHandling;

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
}
