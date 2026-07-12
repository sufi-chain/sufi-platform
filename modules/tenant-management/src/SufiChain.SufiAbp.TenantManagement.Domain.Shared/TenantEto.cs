using System;
using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.TenantManagement;

[Serializable]
public class TenantEto : IHasEntityVersion
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int EntityVersion { get; set; }
}
