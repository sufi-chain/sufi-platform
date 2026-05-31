namespace SufiChain.SufiAbp.UI.Abstractions.Account;

/// <summary>
/// Stores a short-lived pending login after password validation when two-factor authentication is required.
/// Used by Blazor Interactive Server where <see cref="SignInManager{TUser}.PasswordSignInAsync"/> cannot set the 2FA cookie in-circuit.
/// </summary>
public interface ITwoFactorPendingLoginStore
{
    /// <summary>
    /// When true, the host registered a real store for the interactive 2FA login flow.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Creates a pending login token (typically 5 minutes TTL).
    /// </summary>
    Task<string> CreateAsync(
        Guid userId,
        string? returnUrl,
        bool rememberMe = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads pending login data without consuming the token.
    /// </summary>
    Task<(Guid userId, string? returnUrl, bool rememberMe)?> GetAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consumes the token and returns pending login data, or null if invalid/expired.
    /// </summary>
    Task<(Guid userId, string? returnUrl, bool rememberMe)?> ConsumeAsync(
        string token,
        CancellationToken cancellationToken = default);
}
