using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// The public origin short links and invitation destinations are built on.
/// </summary>
public interface IShortLinkPublicOriginResolver
{
    Task<string?> GetOriginAsync();
}
