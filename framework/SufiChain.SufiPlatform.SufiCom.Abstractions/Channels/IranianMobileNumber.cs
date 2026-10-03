using System.Text;

namespace SufiChain.SufiPlatform.SufiCom.Channels;

/// <summary>
/// Normalizes Iranian mobile numbers to the formats Iranian SMS gateways expect.
/// </summary>
public static class IranianMobileNumber
{
    /// <summary>
    /// Returns the 10-digit national significant number (for example <c>9121234567</c>),
    /// or <c>null</c> when <paramref name="phone"/> is not an Iranian mobile number.
    /// Accepts <c>+98</c>, <c>0098</c>, <c>98</c>, <c>0</c> prefixes and Persian or Arabic digits.
    /// </summary>
    public static string? ToNationalNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var digits = new StringBuilder(phone!.Length);
        foreach (var ch in phone)
        {
            if (ch >= '0' && ch <= '9')
            {
                digits.Append(ch);
            }
            else if (ch >= '\u06F0' && ch <= '\u06F9')
            {
                digits.Append((char)('0' + (ch - '\u06F0')));
            }
            else if (ch >= '\u0660' && ch <= '\u0669')
            {
                digits.Append((char)('0' + (ch - '\u0660')));
            }
        }

        var value = digits.ToString();
        if (value.StartsWith("0098", StringComparison.Ordinal))
        {
            value = value.Substring(4);
        }
        else if (value.StartsWith("98", StringComparison.Ordinal) && value.Length == 12)
        {
            value = value.Substring(2);
        }
        else if (value.StartsWith("0", StringComparison.Ordinal) && value.Length == 11)
        {
            value = value.Substring(1);
        }

        return value.Length == 10 && value[0] == '9' ? value : null;
    }

    /// <summary>
    /// Local format with a leading zero (<c>09121234567</c>); returns the input when it is not an Iranian mobile number.
    /// </summary>
    public static string ToLocal(string phone)
    {
        var national = ToNationalNumber(phone);
        return national == null ? phone.Trim() : "0" + national;
    }

    /// <summary>
    /// International format (<c>+989121234567</c>); returns the input when it is not an Iranian mobile number.
    /// </summary>
    public static string ToInternational(string phone)
    {
        var national = ToNationalNumber(phone);
        return national == null ? phone.Trim() : "+98" + national;
    }
}
