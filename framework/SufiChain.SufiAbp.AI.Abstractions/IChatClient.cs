using Microsoft.Extensions.AI;

namespace SufiChain.SufiAbp.AI;

public interface IChatClient<TWorkSpace> : IChatClient
    where TWorkSpace : class
{
    
}
