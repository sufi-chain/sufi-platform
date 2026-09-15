using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Controllers;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KnowledgeBaseHttpApiContractTests
{
    [Theory]
    [InlineData(typeof(KBArticleController), "api/helpdesk/kb/articles")]
    [InlineData(typeof(KBArticleVersionController), "api/helpdesk/kb/articles/{articleId:guid}/versions")]
    [InlineData(typeof(KBProjectController), "api/helpdesk/kb/projects")]
    [InlineData(typeof(KBSearchController), "api/helpdesk/kb/search")]
    [InlineData(typeof(KBArticleIndexingStatusController), "api/helpdesk/kb/articles/indexing-status")]
    [InlineData(typeof(KBRagSmokeTestController), "api/helpdesk/kb/ai/rag-smoke-test")]
    public void Should_Use_Stable_KnowledgeBase_Routes(Type controllerType, string route)
    {
        controllerType.GetCustomAttribute<RouteAttribute>()!.Template.ShouldBe(route);
    }

    [Fact]
    public void Article_Controller_Should_Expose_Public_Slug_Route()
    {
        typeof(KBArticleController)
            .GetMethod(nameof(KBArticleController.GetBySlugAsync))!
            .GetCustomAttribute<HttpGetAttribute>()!
            .Template
            .ShouldBe("public/{projectId:guid}/{languageCode}/{slug}");
    }

    [Fact]
    public void Version_Controller_Should_Expose_Restore_Route()
    {
        typeof(KBArticleVersionController)
            .GetMethod(nameof(KBArticleVersionController.RestoreAsync))!
            .GetCustomAttribute<HttpPostAttribute>()!
            .Template
            .ShouldBe("{versionNumber:int}/restore");
    }
}
