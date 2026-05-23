using System;
using System.Collections.Generic;
using Volo.Abp.EventBus;

namespace SufiChain.SufiAbp.PermissionManagement;

[Serializable]
[EventName("abp.permission-management.dynamic-permission-definitions-changed")]
public class DynamicPermissionDefinitionsChangedEto
{
    public List<string> Permissions { get; set; }
}
