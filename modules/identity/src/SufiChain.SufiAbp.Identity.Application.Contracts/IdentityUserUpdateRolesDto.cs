using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.Identity;

public class IdentityUserUpdateRolesDto
{
    [Required]
    public string[] RoleNames { get; set; } = Array.Empty<string>();
}
