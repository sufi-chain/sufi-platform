using System.Security.Cryptography;
using System.Text;

namespace SufiChain.SufiAbp.Account.Otp;

internal static class OtpCodeHasher
{
    public static string Hash(string code)
    {
        var normalized = code.Trim();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes);
    }
}
