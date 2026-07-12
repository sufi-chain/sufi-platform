using System.Security.Cryptography;
using System.Text;

namespace SufiChain.SufiPlatform.Captcha;

internal static class CaptchaAnswerHasher
{
    public static string Hash(string answer)
    {
        var normalized = answer.Trim();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes);
    }
}
