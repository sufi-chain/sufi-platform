using System;

namespace SufiChain.SufiPlatform.Tags.Relations;

/// <summary>
/// Trusted adapter output. Contains no titles, URLs or source content that could
/// disclose a denied endpoint. CanRelate never substitutes for CanRead.
/// </summary>
public sealed class TagRelationEntityResolution
{
    public TagEntityReference Reference { get; }
    public Guid? TenantId { get; }
    public TagEntityReference Scope { get; }
    public bool CanRead { get; }
    public bool CanRelate { get; }

    public TagRelationEntityResolution(
        TagEntityReference reference,
        Guid? tenantId,
        TagEntityReference scope,
        bool canRead,
        bool canRelate)
    {
        Reference = reference ?? throw new ArgumentNullException(nameof(reference));
        TenantId = tenantId;
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        CanRead = canRead;
        CanRelate = canRelate;
    }
}
