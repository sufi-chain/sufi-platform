using System.Collections.Generic;
using SufiChain.SufiAbp.AIManagement.MCP.Tools;

namespace SufiChain.SufiAbp.AIManagement.Workspaces;

public class WorkspaceMCPToolConfigurationDto
{
    public List<MCPToolDto> AvailableTools { get; set; } = new();

    public List<string> EnabledToolNames { get; set; } = new();
}

public class UpdateWorkspaceMCPToolConfigurationDto
{
    public List<string> EnabledToolNames { get; set; } = new();
}
