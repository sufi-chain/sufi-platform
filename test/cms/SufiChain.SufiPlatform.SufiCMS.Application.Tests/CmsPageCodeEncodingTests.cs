using System.Text;
using Shouldly;
using SufiChain.SufiPlatform.SufiCMS.Html;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

public class CmsPageCodeEncodingTests
{
    private const string Persian = "سلام";
    private const string Arabic = "مرحبا";

    [Fact]
    public void Normalize_Round_Trips_Persian_And_Arabic_In_Page_Js_And_Css()
    {
        var js = "const title = \"" + Persian + "\"; const greeting = \"" + Arabic + "\";";
        var css = """
            @font-face {
              font-family: "Vazirmatn";
              src: url("/fonts/vazirmatn.woff2") format("woff2");
            }
            .hero {
              font-family: "Vazirmatn", sans-serif;
              direction: rtl;
              margin-left: 1rem;
              margin-right: 2rem;
              left: 0;
              right: 4px;
            }
            .hero::before { content: "سلام"; }
            .hero::after { content: "مرحبا"; }
            """;

        CmsPageCodeEncoding.Normalize(js).ShouldBe(js);
        CmsPageCodeEncoding.Normalize(css).ShouldBe(css);
        CmsPageCodeEncoding.Normalize(null).ShouldBeNull();
        CmsPageCodeEncoding.Normalize(string.Empty).ShouldBe(string.Empty);
        CmsPageCodeEncoding.Normalize("const n = 1;").ShouldBe("const n = 1;");
    }

    [Fact]
    public void Normalize_Repairs_Latin1_Misread_Of_Utf8_In_Page_Js_And_Css()
    {
        var js = "const title = \"" + Persian + "\"; const greeting = \"" + Arabic + "\";";
        var css = ".hero::before { content: \"" + Persian + "\"; } .hero::after { content: \"" + Arabic + "\"; }";

        CmsPageCodeEncoding.Normalize(AsLatin1OfUtf8(js)).ShouldBe(js);
        CmsPageCodeEncoding.Normalize(AsLatin1OfUtf8(css)).ShouldBe(css);
        CmsPageCodeEncoding.Normalize(AsLatin1OfUtf8(AsLatin1OfUtf8(js))).ShouldBe(js);
    }

    [Fact]
    public void Normalize_Leaves_A_Real_Latin1_Character_That_Is_Not_Utf8()
    {
        const string cafe = "caf\u00e9";
        CmsPageCodeEncoding.Normalize(cafe).ShouldBe(cafe);
    }

    private static string AsLatin1OfUtf8(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Encoding.Latin1.GetString(bytes);
    }
}
