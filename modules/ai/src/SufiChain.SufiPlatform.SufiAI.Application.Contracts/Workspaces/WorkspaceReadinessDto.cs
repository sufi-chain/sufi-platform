namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceReadinessDto
{
    public Guid WorkspaceId { get; set; }

    public string WorkspaceName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool IsConfigured { get; set; }

    public bool IsReady { get; set; }

    public List<WorkspaceCapabilityReadinessDto> Capabilities { get; set; } = new();

    public WorkspaceMcpReadinessDto Mcp { get; set; } = new();
}

public class WorkspaceCapabilityReadinessDto
{
    public AICapabilityType CapabilityType { get; set; }

    public bool IsConfigured { get; set; }

    public bool IsReady { get; set; }

    public AIProviderType Provider { get; set; }
    public OpenAIApiMode? OpenAIApiMode { get; set; }

    public string? ModelId { get; set; }

    public bool HasApiEndpoint { get; set; }

    public bool HasApiKey { get; set; }

    public bool UsesWorkspaceFallback { get; set; }

    public string? FailureCode { get; set; }
}

public class WorkspaceMcpReadinessDto
{
    public bool IsConfigured { get; set; }

    public bool IsReady { get; set; }

    public AIProviderType Provider { get; set; }

    public string? ModelId { get; set; }

    public OpenAIApiMode OpenAIApiMode { get; set; }

    public string? FailureCode { get; set; }
}
