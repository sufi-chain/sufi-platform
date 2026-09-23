using System.Text;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare.Knowledge;

public class PersianTextNormalizerTests
{
    [Fact]
    public void Should_Map_Arabic_Yeh_And_Kaf_To_Persian()
    {
        PersianTextNormalizer.Normalize("علي كتاب ى").ShouldBe("علی کتاب ی");
    }

    [Fact]
    public void Should_Fold_Presentation_Forms()
    {
        // U+FEB3 U+FEE0 U+FE8E U+FEE1 are presentation forms of "سلام".
        PersianTextNormalizer.Normalize("\uFEB3\uFEE0\uFE8E\uFEE1").ShouldBe("سلام");
    }

    [Fact]
    public void Should_Keep_Zwnj_And_Collapse_Repeats()
    {
        PersianTextNormalizer.Normalize("می\u200C\u200Cشود").ShouldBe("می\u200Cشود");
        PersianTextNormalizer.Normalize("می \u200Cشود").ShouldBe("می شود");
    }

    [Fact]
    public void Should_Strip_Tatweel_Bidi_Controls_And_Zero_Width_Joiners()
    {
        PersianTextNormalizer.Normalize("\u202Bکـــتاب\u202C\u200E \u200Dنو\u200B").ShouldBe("کتاب نو");
    }

    [Fact]
    public void Should_Keep_Mixed_Script_Digits_And_Markdown_Structure()
    {
        var input = "# عنوان\r\n\r\n\r\n\r\n- مورد ۱ Version 2.0   و  50 نفر  \n  - زیرمورد\n| a | b |";

        var result = PersianTextNormalizer.Normalize(input);

        result.ShouldBe("# عنوان\n\n- مورد ۱ Version 2.0 و 50 نفر\n  - زیرمورد\n| a | b |");
    }

    [Fact]
    public void Should_Return_Empty_For_Null()
    {
        PersianTextNormalizer.Normalize(null).ShouldBe(string.Empty);
    }
}

public class HooshvareTextDecoderTests
{
    private const string Persian = "سلام دنیا، این یک متن فارسی است.";

    static HooshvareTextDecoderTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Fact]
    public void Should_Decode_Utf8_With_And_Without_Bom_And_Windows1256_Identically()
    {
        var withBom = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(Persian)).ToArray();
        var withoutBom = new UTF8Encoding(false).GetBytes(Persian);
        // Windows-1256 has no Persian Yeh (U+06CC); legacy files store Arabic Yeh, which normalization maps back.
        var windows1256 = Encoding.GetEncoding(1256).GetBytes(Persian.Replace('\u06CC', '\u064A'));

        var decoded = new[] { withBom, withoutBom, windows1256 }
            .Select(bytes => PersianTextNormalizer.Normalize(HooshvareTextDecoder.Decode(bytes)))
            .ToList();

        decoded.ShouldAllBe(text => text == Persian);
    }

    [Fact]
    public void Should_Decode_Utf16_By_Bom()
    {
        var bytes = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes(Persian)).ToArray();

        HooshvareTextDecoder.Decode(bytes).ShouldBe(Persian);
    }
}
