namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Maps an upstream HTTP status onto the business error the chat, CMS Writer, and SEO shortcuts show.
/// Other statuses keep the generic provider-failure text, which still fills <c>{StatusCode}</c>.
/// </summary>
public static class ProviderFailureKind
{
    public static string CodeFor(int? status) => status switch
    {
        401 or 403 => AIErrorCodes.ProviderAuthorizationFailed,
        429 => AIErrorCodes.ProviderRateLimited,
        >= 500 and <= 599 => AIErrorCodes.ProviderUnavailable,
        not null => AIErrorCodes.ProviderRequestFailed,
        _ => AIErrorCodes.ProviderRequestFailedBrief
    };

    public static bool IsGenericProviderFailure(string? code) =>
        code is AIErrorCodes.ProviderRequestFailed or AIErrorCodes.ProviderRequestFailedBrief;
}
