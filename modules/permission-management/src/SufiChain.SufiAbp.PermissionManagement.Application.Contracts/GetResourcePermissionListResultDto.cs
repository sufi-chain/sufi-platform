using System.Collections.Generic;

namespace SufiChain.SufiAbp.PermissionManagement;

public class GetResourcePermissionListResultDto
{
    public List<ResourcePermissionGrantInfoDto> Permissions { get; set; }
}
