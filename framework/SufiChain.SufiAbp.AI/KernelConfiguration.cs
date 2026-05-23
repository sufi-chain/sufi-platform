using Microsoft.SemanticKernel;

namespace SufiChain.SufiAbp.AI;

public class KernelConfiguration
{
    public IKernelBuilder? Builder { get; set; }
    public KernelBuilderConfigurerList BuilderConfigurers { get; } = new();
}
