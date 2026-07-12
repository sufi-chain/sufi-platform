using SufiChain.SufiPlatform.UI.Abstractions.Account;

namespace SufiChain.SufiPlatform.Account.Blazor.Services;

/// <summary>
/// No-op pending 2FA store for hosts that use static SSR sign-in (SignInManager cookie flow).
/// </summary>
public class NullTwoFactorPendingLoginStore : ITwoFactorPendingLoginStore
{
    public bool IsSupported => false;

    public Task<string> CreateAsync(
        Guid userId,
        string? returnUrl,
        bool rememberMe = false,
        CancellationToken cancellationToken = default)
        => Task.FromResult(string.Empty);

    public Task<(Guid userId, string? returnUrl, bool rememberMe)?> GetAsync(
        string token,
        CancellationToken cancellationToken = default)
        => Task.FromResult<(Guid, string?, bool)?>(null);

    public Task<(Guid userId, string? returnUrl, bool rememberMe)?> ConsumeAsync(
        string token,
        CancellationToken cancellationToken = default)
        => Task.FromResult<(Guid, string?, bool)?>(null);
}
