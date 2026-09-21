using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Account.Localization;

namespace SufiChain.SufiPlatform.Account.Blazor;

internal static class AccountUiErrors
{
    public static string LocalizedFailure(
        ILogger logger,
        IStringLocalizer<SufiAccountResource> accountL,
        Exception exception,
        string localizationKey)
    {
        logger.LogError(exception, "Account UI operation failed ({Key}).", localizationKey);
        return accountL[localizationKey];
    }
}
