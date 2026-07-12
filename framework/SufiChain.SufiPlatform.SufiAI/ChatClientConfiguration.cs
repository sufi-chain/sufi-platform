using Microsoft.Extensions.AI;

namespace SufiChain.SufiPlatform.SufiAI;

public class ChatClientConfiguration
{
    public ChatClientBuilder? Builder { get; set; }
    public BuilderConfigurerList BuilderConfigurers { get; } = new();
}
