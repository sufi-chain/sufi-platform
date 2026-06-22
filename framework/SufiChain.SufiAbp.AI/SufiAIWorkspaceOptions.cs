namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Pre-configured options for the AI workspaces. Not used via Options pattern. Use it with 'PreConfigure' method in a Module class.
/// In example:
/// <code>PreConfigure&lt;SufiAIWorkspaceOptions&gt;(options => { });</code>
/// </summary>
public class SufiAIWorkspaceOptions
{
    public const string ChatClientServiceKeyNamePrefix = "SufiAbp.AI.ChatClient_";
    public const string KernelServiceKeyNamePrefix = "SufiAbp.AI.Kernel_";
    
    public WorkspaceConfigurationDictionary Workspaces { get; } = new();

    public static string GetChatClientServiceKeyName(string name)
    {
        return $"{ChatClientServiceKeyNamePrefix}{name}";
    }

    public static string GetKernelServiceKeyName(string name)
    {
        return $"{KernelServiceKeyNamePrefix}{name}";
    }
}
