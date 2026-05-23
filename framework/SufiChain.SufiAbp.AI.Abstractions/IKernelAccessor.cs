using Microsoft.SemanticKernel;

namespace SufiChain.SufiAbp.AI;

public interface IKernelAccessor
{
    Kernel? Kernel { get; }
}

public interface IKernelAccessor<TWorkSpace> : IKernelAccessor
    where TWorkSpace : class
{
}
