using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.ShortLinks;

public interface IShortUrlPublicRedirector
{
    Task<ShortUrlPublicRedirect?> TryRedirectAsync(
        string? shortCode,
        string? token,
        string? userAgent,
        string? ipAddress,
        string? referrer);
}
