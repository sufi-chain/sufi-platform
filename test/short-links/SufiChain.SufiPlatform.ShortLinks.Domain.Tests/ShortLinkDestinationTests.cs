using System.Text;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortLinkDestinationTests
{
    [Theory]
    [InlineData("en", "absolute http or https URL")]
    [InlineData("en-GB", "absolute http or https URL")]
    [InlineData("fa", "نشانی مطلق http یا https")]
    [InlineData("ar", "http أو https")]
    [InlineData("es", "URL absoluta http o https")]
    public void Invalid_destination_message_names_http_and_https(string culture, string expected)
    {
        var assembly = typeof(ShortLinksErrorCodes).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .Single(name => name.EndsWith($".{culture}.json", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName);
        stream.ShouldNotBeNull();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var json = reader.ReadToEnd();
        json.ShouldContain("ShortLinks:InvalidDestinationUrl");
        json.ShouldContain(expected);
    }

    [Theory]
    [InlineData("https://example.com/docs", true)]
    [InlineData("http://example.com", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("data:text/html,hi", false)]
    [InlineData(" https://example.com\r\nLocation: https://evil.example", false)]
    public void Absolute_http_check_rejects_other_schemes(string destination, bool expected)
    {
        ShortLinkRedirectHelper.IsAbsoluteHttpUrl(destination).ShouldBe(expected);
    }
}
