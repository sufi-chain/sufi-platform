using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class CloneWorkspaceDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;
}
