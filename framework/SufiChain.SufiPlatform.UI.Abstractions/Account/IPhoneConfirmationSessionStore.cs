namespace SufiChain.SufiPlatform.UI.Abstractions.Account;

/// <summary>
/// Server-side proof that the caller may confirm the phone of one user.
/// Issued only after registration, a verified password, or a verified email confirmation.
/// The phone confirmation API resolves the user from this token. It does not accept a raw user id.
/// </summary>
public interface IPhoneConfirmationSessionStore
{
    /// <summary>
    /// When false, the host has no shared store and callers must not start phone confirmation.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Creates a reusable token for <paramref name="userId"/>. The token expires after a short absolute lifetime.
    /// </summary>
    Task<string> CreateAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the user id bound to the token, or null when the token is missing, unknown, or expired.
    /// The token stays valid until it expires or <see cref="RevokeAsync"/> removes it.
    /// </summary>
    Task<Guid?> FindUserIdAsync(string? sessionToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the token so it cannot be used again.
    /// </summary>
    Task RevokeAsync(string? sessionToken, CancellationToken cancellationToken = default);
}
