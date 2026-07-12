using Microsoft.Extensions.AI;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Typed keyed chat client bound to a workspace marker type.
/// <para>
/// Framework/advanced use only: this contract exposes the
/// <c>Microsoft.Extensions.AI</c> SDK surface. Product modules must not consume it;
/// they use <see cref="ISufiAIChatService"/> and the other Sufi DTO-based
/// contracts instead.
/// </para>
/// </summary>
public interface IChatClient<TWorkSpace> : IChatClient
    where TWorkSpace : class
{
    
}
