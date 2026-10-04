using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Identity;

public interface ILinkLoginTokenFailureDescriber
{
    Task<string> DescribeAsync(
        IdentityLinkUserInfo source,
        string token,
        string purpose,
        CancellationToken cancellationToken = default);
}
