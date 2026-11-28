namespace SufiChain.SufiPlatform.Captcha;

/// <summary>
/// Maps Persian and Arabic-Indic digits to Latin digits before a captcha answer is compared,
/// and the other way when the question is shown in Persian.
/// </summary>
public static class CaptchaDigits
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var chars = value.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = ToLatin(chars[i]);
        }

        return new string(chars);
    }

    public static string ToPersian(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var chars = value.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] is >= '0' and <= '9')
            {
                chars[i] = (char)('\u06F0' + (chars[i] - '0'));
            }
        }

        return new string(chars);
    }

    private static char ToLatin(char value)
    {
        if (value is >= '\u06F0' and <= '\u06F9')
        {
            return (char)('0' + (value - '\u06F0'));
        }

        if (value is >= '\u0660' and <= '\u0669')
        {
            return (char)('0' + (value - '\u0660'));
        }

        return value;
    }
}
