using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.FileManager.FileStructures;

/// <summary>
/// Quick Share's sibling rule for a structure key. The validation error code is the
/// localization key <see cref="FormatInvalid"/> in en, fa, ar, and es.
/// </summary>
public static class FileStructureKeyRules
{
    public const string FormatInvalid = "KeyFormatInvalid";

    public const string Pattern = @"^[A-Za-z0-9][\w.]*$";

    private static readonly Regex KeyPattern = new(Pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static bool IsFormatValid(string? key)
    {
        return !string.IsNullOrEmpty(key) && KeyPattern.IsMatch(key);
    }
}
