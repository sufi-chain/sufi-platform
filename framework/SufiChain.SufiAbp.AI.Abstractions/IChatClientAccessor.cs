using Microsoft.Extensions.AI;

namespace SufiChain.SufiAbp.AI;

public interface IChatClientAccessor
{
    IChatClient? ChatClient { get; }
}

public interface IChatClientAccessor<TWorkSpace> : IChatClientAccessor
    where TWorkSpace : class
{
}
