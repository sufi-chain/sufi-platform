using System.Text.Json;
using Shouldly;
using SufiChain.SufiPlatform.Account.Otp;
using Xunit;

namespace SufiChain.SufiPlatform.Account.Otp;

public class OtpMessageKeysTests
{
    [Theory]
    [InlineData(VerificationDeliveryChannel.Email, "OtpCodeSentEmail")]
    [InlineData(VerificationDeliveryChannel.Sms, "OtpCodeSentPhone")]
    [InlineData(VerificationDeliveryChannel.Voice, "OtpCodeSentPhone")]
    public void CodeSent_picks_email_or_phone_key(VerificationDeliveryChannel channel, string expected)
    {
        OtpMessageKeys.CodeSent(channel).ShouldBe(expected);
    }

    [Theory]
    [InlineData("en.json")]
    [InlineData("fa.json")]
    [InlineData("ar.json")]
    [InlineData("es.json")]
    public void Account_resources_include_channel_code_sent_keys(string fileName)
    {
        var texts = LoadTexts(fileName);
        texts.ShouldContainKey("OtpCodeSentEmail");
        texts.ShouldContainKey("OtpCodeSentPhone");
        texts["OtpCodeSentEmail"].ShouldNotBeNullOrWhiteSpace();
        texts["OtpCodeSentPhone"].ShouldNotBeNullOrWhiteSpace();
    }

    private static Dictionary<string, string> LoadTexts(string fileName)
    {
        var path = FindRepoFile(Path.Combine(
            "modules",
            "account",
            "src",
            "SufiChain.SufiPlatform.Account.Domain.Shared",
            "Localization",
            "Account",
            fileName));
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in document.RootElement.GetProperty("texts").EnumerateObject())
        {
            map[property.Name] = property.Value.GetString() ?? string.Empty;
        }

        return map;
    }

    private static string FindRepoFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            foreach (var candidate in new[]
                     {
                         Path.Combine(dir.FullName, relativePath),
                         Path.Combine(dir.FullName, "sufi-platform", relativePath)
                     })
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relativePath);
    }
}
