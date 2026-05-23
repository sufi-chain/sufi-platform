using Microsoft.Extensions.AI;

namespace SufiChain.SufiAbp.AI;

public class ChatClientConfiguration
{
    public ChatClientBuilder? Builder { get; set; }
    public BuilderConfigurerList BuilderConfigurers { get; } = new();
}
