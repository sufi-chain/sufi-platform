using Microsoft.SemanticKernel;

namespace SufiChain.SufiPlatform.SufiAI;

public class KernelConfiguration
{
    public IKernelBuilder? Builder { get; set; }
    public KernelBuilderConfigurerList BuilderConfigurers { get; } = new();
}
