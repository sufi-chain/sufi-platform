using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityUserUpdateRolesDto
{
    [Required]
    public string[] RoleNames { get; set; } = Array.Empty<string>();
}
