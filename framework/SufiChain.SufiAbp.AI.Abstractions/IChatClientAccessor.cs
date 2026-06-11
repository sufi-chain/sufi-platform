using Microsoft.Extensions.AI;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Resolves the raw <see cref="IChatClient"/> for the default workspace.
/// <para>
/// Framework/advanced use only: this contract exposes the
/// <c>Microsoft.Extensions.AI</c> SDK surface. Product modules must not consume it;
/// they use <see cref="ISufiAbpAIChatService"/> and the other SufiAbp DTO-based
/// contracts instead.
/// </para>
/// </summary>
public interface IChatClientAccessor
{
    /// <summary>
    /// The chat client, or <c>null</c> when no provider is configured.
    /// </summary>
    IChatClient? ChatClient { get; }
}

/// <summary>
/// Resolves the raw <see cref="IChatClient"/> for the workspace identified by
/// <typeparamref name="TWorkSpace"/>. Framework/advanced use only — see
/// <see cref="IChatClientAccessor"/>.
/// </summary>
public interface IChatClientAccessor<TWorkSpace> : IChatClientAccessor
    where TWorkSpace : class
{
}
