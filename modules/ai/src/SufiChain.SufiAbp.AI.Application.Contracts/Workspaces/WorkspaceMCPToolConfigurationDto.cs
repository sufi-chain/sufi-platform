using System.Collections.Generic;
using SufiChain.SufiAbp.AI.MCP.Tools;

namespace SufiChain.SufiAbp.AI.Workspaces;

public class WorkspaceMCPToolConfigurationDto
{
    public List<MCPToolDto> AvailableTools { get; set; } = new();

    public List<string> EnabledToolNames { get; set; } = new();
}

public class UpdateWorkspaceMCPToolConfigurationDto
{
    public List<string> EnabledToolNames { get; set; } = new();
}
