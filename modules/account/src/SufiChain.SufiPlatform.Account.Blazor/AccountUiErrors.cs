using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Account.Blazor.Components;
using SufiChain.SufiPlatform.Account.Blazor.Pages;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.ExceptionHandling;

namespace SufiChain.SufiPlatform.Account.Blazor;

public static class AccountUiErrors
{
    private const string CooldownSecondsKey = "seconds";
    public const string OtpCaptchaFailedKey = "OtpCaptchaFailed";

    public static string LocalizedFailure(
        ILogger logger,
        IStringLocalizer<SufiAccountResource> accountL,
        Exception exception,
        string localizationKey)
    {
        var code = exception is IHasErrorCode hasCode ? hasCode.Code : null;
        if (string.IsNullOrWhiteSpace(code))
        {
            logger.LogError(exception, "Account UI operation failed ({Key}).", localizationKey);
        }
        else
        {
            logger.LogError(exception, "Account UI operation failed ({Key}). Code={Code}.", localizationKey, code);
        }

        return accountL[localizationKey];
    }

    /// <summary>
    /// Handles OTP send failures: a resend cooldown yields a wait message and the time resend becomes available;
    /// anything else falls back to <see cref="LocalizedFailure"/>.
    /// </summary>
    public static string OtpSendFailure(
        ILogger logger,
        IStringLocalizer<SufiAccountResource> accountL,
        Exception exception,
        string localizationKey,
        out DateTimeOffset? resendAvailableAt)
    {
        if (TryGetResendCooldownSeconds(exception, out var seconds))
        {
            resendAvailableAt = DateTimeOffset.UtcNow.AddSeconds(seconds);
            return accountL["OtpResendCooldownWait", OtpResendButton.FormatRemaining(seconds)];
        }

        resendAvailableAt = null;
        var key = exception is IHasErrorCode { Code: var code } &&
                  string.Equals(code, ForgotPasswordFeedback.CaptchaErrorKey, StringComparison.Ordinal)
            ? OtpCaptchaFailedKey
            : localizationKey;
        return LocalizedFailure(logger, accountL, exception, key);
    }

    public static bool TryGetResendCooldownSeconds(Exception exception, out int seconds)
    {
        seconds = 0;
        if (exception is not IHasErrorCode { Code: IdentitySecurityErrorCodes.OtpResendCooldown })
        {
            return false;
        }

        var value = exception.Data[CooldownSecondsKey];
        if (!int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds) ||
            seconds <= 0)
        {
            seconds = 1;
        }

        return true;
    }

    public static DateTimeOffset? ToResendAvailableAt(OtpSendResultDto result)
    {
        return result.ResendAfterSeconds > 0
            ? DateTimeOffset.UtcNow.AddSeconds(result.ResendAfterSeconds)
            : null;
    }
}
