using Microsoft.Extensions.Logging;
using Volo.Abp.ExceptionHandling;

namespace SufiChain.SufiPlatform.UI.Blazor.ExceptionHandling;

/// <summary>
/// Writes handled UI exceptions with the ABP error code when the exception has one.
/// </summary>
public static class SufiExceptionLog
{
    public static void LogError(ILogger logger, Exception exception, string messagePrefix)
    {
        var code = (exception as IHasErrorCode)?.Code;
        logger.LogError(
            exception,
            messagePrefix + ": {Message} Code={Code}",
            exception.Message,
            code);
    }
}
