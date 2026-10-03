using System.Security.Cryptography;

namespace SufiChain.SufiPlatform.Account.Otp;

public static class OtpCodeGenerator
{
    public const int DefaultLength = 6;

    public static string Generate(int length)
    {
        if (length < 4)
        {
            length = DefaultLength;
        }

        var digits = new char[length];
        for (var i = 0; i < length; i++)
        {
            digits[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        }

        return new string(digits);
    }
}
