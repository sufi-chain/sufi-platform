using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class UpdateWorkspaceGuardrailsDto
{
    public List<WorkspaceGuardrailDto> Items { get; set; } = new();
}
