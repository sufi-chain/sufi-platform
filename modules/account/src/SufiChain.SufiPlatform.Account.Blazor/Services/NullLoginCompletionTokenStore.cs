using SufiChain.SufiPlatform.UI.Abstractions.Account;

namespace SufiChain.SufiPlatform.Account.Blazor.Services;

/// <summary>
/// No-op implementation of <see cref="ILoginCompletionTokenStore"/>.
/// Hosts that use Interactive Server with cookie auth should replace this with a real implementation
/// (e.g. one that uses IMemoryCache) so that /account/complete-login can set the auth cookie.
/// </summary>
public class NullLoginCompletionTokenStore : ILoginCompletionTokenStore
{
    public bool IsSupported => false;

    public Task<string> CreateAsync(Guid userId, string? returnUrl, bool rememberMe = false, CancellationToken cancellationToken = default)
        => Task.FromResult(string.Empty);

    public Task<(Guid userId, string? returnUrl, bool rememberMe)?> ConsumeAsync(string token, CancellationToken cancellationToken = default)
        => Task.FromResult<(Guid, string?, bool)?>(null);
}
