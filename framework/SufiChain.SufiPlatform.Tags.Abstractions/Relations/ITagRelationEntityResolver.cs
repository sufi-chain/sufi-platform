using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Tags.Relations;

/// <summary>
/// An owning-module adapter, executed under the current principal and tenant.
/// Implementations must recheck current access, not trust cached access decisions.
/// Return only available endpoints; missing and denied endpoints are indistinguishable.
/// The scope is resolved from the owning record, never supplied by a tool or client.
/// </summary>
public interface ITagRelationEntityResolver
{
    string EntityType { get; }

    Task<IReadOnlyList<TagRelationEntityResolution>> ResolveAsync(
        IReadOnlyList<TagEntityReference> references,
        CancellationToken cancellationToken = default);
}
