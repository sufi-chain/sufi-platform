using System;
using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.Identity;

[Serializable]
public class IdentityClaimTypeEto : IHasEntityVersion
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public bool Required { get; set; }

    public bool IsStatic { get; set; }

    public string? Regex { get; set; }

    public string? RegexDescription { get; set; }

    public string? Description { get; set; }

    public int EntityVersion { get; set; }
}
