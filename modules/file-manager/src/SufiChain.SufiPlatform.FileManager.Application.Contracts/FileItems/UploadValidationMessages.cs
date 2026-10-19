using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.FileManager.Localization;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Turns an upload rejection into text in the user's language (sufi-chain#479: the zip rejection showed
/// "File type 'application/zip' is not allowed" in English inside the fa UI).
/// </summary>
public static class UploadValidationMessages
{
    public static string? Localize(IStringLocalizer localizer, UploadValidationResult result, CultureInfo? culture = null)
    {
        if (string.IsNullOrWhiteSpace(result.ErrorKey))
        {
            return result.ErrorMessage;
        }

        var args = result.ErrorArgs
            .Select(arg => arg is int or long ? FileManagerDigits.Localize(System.Convert.ToString(arg, CultureInfo.InvariantCulture), culture) : arg)
            .ToArray();
        var localized = localizer[result.ErrorKey, args];
        return localized.ResourceNotFound ? result.ErrorMessage : localized.Value;
    }
}
