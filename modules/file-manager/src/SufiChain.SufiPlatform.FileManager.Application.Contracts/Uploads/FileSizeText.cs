using System;
using System.Globalization;
using SufiChain.SufiPlatform.FileManager.Localization;

namespace SufiChain.SufiPlatform.FileManager.Uploads;

/// <summary>
/// File sizes in the user's language (sufi-chain#479: the limit read "100 MB" in the fa UI instead of «۱۰۰ مگابایت»).
/// </summary>
public static class FileSizeText
{
    private static readonly string[] UnitKeys = { "FileSize:B", "FileSize:KB", "FileSize:MB", "FileSize:GB" };

    public static (string Number, string UnitKey) Split(long bytes)
    {
        double value = Math.Max(0, bytes);
        var order = 0;
        while (value >= 1024 && order < UnitKeys.Length - 1)
        {
            order++;
            value /= 1024;
        }

        return (value.ToString("0.##", CultureInfo.InvariantCulture), UnitKeys[order]);
    }

    /// <summary>Formats a size; <c>localize</c> resolves a unit key with the number as its {0} argument, e.g. <c>(key, n) => L[key, n]</c>.</summary>
    public static string Format(long bytes, Func<string, string, string> localize, CultureInfo? culture = null)
    {
        var (number, unitKey) = Split(bytes);
        return localize(unitKey, FileManagerDigits.Localize(number, culture));
    }
}
