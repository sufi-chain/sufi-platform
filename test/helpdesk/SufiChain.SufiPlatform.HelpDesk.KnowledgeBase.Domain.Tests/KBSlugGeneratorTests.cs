using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Services;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KBSlugGeneratorTests
{
    [Fact]
    public void GenerateSlug_should_normalize_title()
    {
        var generator = new KBSlugGenerator();
        generator.GenerateSlug("Hello Knowledge Base!").ShouldBe("hello-knowledge-base");
    }
}
