#nullable disable
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

namespace SufiChain.SufiPlatform.OpenIddict.Applications;

public interface IOpenIdApplicationStore : IOpenIddictApplicationStore<OpenIddictApplicationModel>
{
    ValueTask<string> GetFrontChannelLogoutUriAsync(OpenIddictApplicationModel application, CancellationToken cancellationToken = default);

    ValueTask<string> GetClientUriAsync(OpenIddictApplicationModel application, CancellationToken cancellationToken = default);

    ValueTask<string> GetLogoUriAsync(OpenIddictApplicationModel application, CancellationToken cancellationToken = default);
}
