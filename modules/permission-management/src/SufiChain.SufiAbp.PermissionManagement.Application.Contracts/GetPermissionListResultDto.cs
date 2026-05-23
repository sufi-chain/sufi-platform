using System.Collections.Generic;

namespace SufiChain.SufiAbp.PermissionManagement;

public class GetPermissionListResultDto
{
    public string EntityDisplayName { get; set; }

    public List<PermissionGroupDto> Groups { get; set; }
}
