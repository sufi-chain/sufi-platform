using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class LoginFieldLocalizationTests
{
    [Theory]
    [InlineData("en")]
    [InlineData("fa")]
    [InlineData("ar")]
    [InlineData("es")]
    public void Required_field_messages_exist_for_each_account_culture(string culture)
    {
        var json = File.ReadAllText(IdentityJson(culture));
        json.ShouldContain("\"UsernameOrEmailRequired\"");
        json.ShouldContain("\"PasswordRequired\"");
    }

    private static string IdentityJson(string culture)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "modules", "identity")))
        {
            dir = dir.Parent;
        }

        dir.ShouldNotBeNull();
        return Path.Combine(
            dir.FullName,
            "modules",
            "identity",
            "src",
            "SufiChain.SufiPlatform.Identity.Domain.Shared",
            "Localization",
            "Identity",
            culture + ".json");
    }
}
