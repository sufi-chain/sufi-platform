namespace SufiChain.SufiPlatform.UI.Abstractions.Account;

/// <summary>
/// Placeholder used until the account application registers the distributed-cache store.
/// </summary>
public class NullPhoneConfirmationSessionStore : IPhoneConfirmationSessionStore
{
    public bool IsSupported => false;

    public Task<string> CreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Phone confirmation sessions are not available.");
    }

    public Task<Guid?> FindUserIdAsync(string? sessionToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Guid?>(null);
    }

    public Task RevokeAsync(string? sessionToken, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
