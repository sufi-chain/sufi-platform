using System.Collections.Generic;

namespace SufiChain.SufiAbp.PermissionManagement;

public class GetResourcePermissionDefinitionListResultDto
{
    public List<ResourcePermissionDefinitionDto> Permissions { get; set; }
}
