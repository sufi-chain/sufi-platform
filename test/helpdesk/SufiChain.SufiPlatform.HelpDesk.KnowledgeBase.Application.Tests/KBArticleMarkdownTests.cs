using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KBArticleMarkdownTests
{
    [Theory]
    [InlineData(typeof(CreateKBArticleDto))]
    [InlineData(typeof(UpdateKBArticleDto))]
    [InlineData(typeof(KBArticleDto))]
    [InlineData(typeof(KBArticleDetailDto))]
    [InlineData(typeof(KBArticleListDto))]
    [InlineData(typeof(KBArticleTranslationDto))]
    [InlineData(typeof(KBArticleVersionDto))]
    [InlineData(typeof(KBArticleVersionListDto))]
    [InlineData(typeof(KBArticle))]
    [InlineData(typeof(KBArticleVersion))]
    public void Article_Contracts_Should_Not_Expose_A_Format_Selector(Type contractType)
    {
        contractType.GetProperty("ContentType").ShouldBeNull();
    }

    [Fact]
    public void Article_Service_Should_Not_Expose_Content_Conversion()
    {
        typeof(IKBArticleAppService).GetMethod("ConvertContentAsync").ShouldBeNull();
        typeof(KBArticleAppService).GetMethod("ConvertContentAsync").ShouldBeNull();
    }

    [Fact]
    public void Display_Should_Render_Markdown()
    {
        var converter = new KBArticleContentConverter();

        converter.RenderForDisplay("# Heading\n\n**Bold**")
            .ShouldContain("<h1");
        converter.RenderForDisplay("# Heading\n\n**Bold**")
            .ShouldContain("<strong>Bold</strong>");
    }

    [Fact]
    public void Display_Should_Not_Pass_Through_Raw_Html()
    {
        var converter = new KBArticleContentConverter();

        converter.RenderForDisplay("<script>alert(1)</script>")
            .ShouldNotContain("<script");
    }

    [Fact]
    public void Html_Import_Should_Produce_Markdown()
    {
        var converter = new KBArticleContentConverter();

        converter.ConvertHtmlToMarkdown("<h1>Heading</h1><p><strong>Bold</strong></p>")
            .ShouldBe("# Heading\n\n**Bold**");
    }

    [Fact]
    public void Versions_Should_Snapshot_Markdown_Content()
    {
        var article = new KBArticle(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var translation = article.AddTranslation(Guid.NewGuid(), "en", "Heading", "heading", "# Heading\n\n**Bold**");

        var version = KBArticleVersion.CreateFromArticle(Guid.NewGuid(), article, translation, "Initial version");

        version.Content.ShouldBe(translation.Content);
        version.LanguageCode.ShouldBe("en");
    }
}
