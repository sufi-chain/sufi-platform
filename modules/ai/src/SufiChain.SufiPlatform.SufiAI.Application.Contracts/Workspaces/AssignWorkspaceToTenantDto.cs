using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class AssignWorkspaceToTenantDto
{
    [Required]
    public Guid TenantId { get; set; }

    [StringLength(128)]
    public string? Name { get; set; }
}
