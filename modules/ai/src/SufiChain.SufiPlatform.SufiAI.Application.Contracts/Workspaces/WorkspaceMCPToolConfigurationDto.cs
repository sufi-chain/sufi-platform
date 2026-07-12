using System.Collections.Generic;
using SufiChain.SufiPlatform.SufiAI.MCP.Tools;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceMCPToolConfigurationDto
{
    public List<MCPToolDto> AvailableTools { get; set; } = new();

    public List<string> EnabledToolNames { get; set; } = new();
}

public class UpdateWorkspaceMCPToolConfigurationDto
{
    public List<string> EnabledToolNames { get; set; } = new();
}
