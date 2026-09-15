using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Permissions;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KBArticleApplicationContractTests
{
    [Theory]
    [InlineData(nameof(KBArticleAppService.GetAsync), KnowledgeBasePermissions.Articles.Default)]
    [InlineData(nameof(KBArticleAppService.GetListAsync), KnowledgeBasePermissions.Articles.Default)]
    [InlineData(nameof(KBArticleAppService.CreateAsync), KnowledgeBasePermissions.Articles.Create)]
    [InlineData(nameof(KBArticleAppService.UpdateAsync), KnowledgeBasePermissions.Articles.Edit)]
    [InlineData(nameof(KBArticleAppService.DeleteAsync), KnowledgeBasePermissions.Articles.Delete)]
    [InlineData(nameof(KBArticleAppService.PublishAsync), KnowledgeBasePermissions.Articles.Publish)]
    [InlineData(nameof(KBArticleAppService.UnpublishAsync), KnowledgeBasePermissions.Articles.Publish)]
    [InlineData(nameof(KBArticleAppService.ArchiveAsync), KnowledgeBasePermissions.Articles.Delete)]
    public void Should_Protect_Article_Application_Methods_With_Expected_Policies(string methodName, string policy)
    {
        var method = typeof(KBArticleAppService)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(x => x.Name == methodName);

        var authorizeAttribute = method.GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute.Policy.ShouldBe(policy);
    }

    [Fact]
    public void Should_Keep_Public_Article_Methods_Anonymous()
    {
        typeof(KBArticleAppService)
            .GetMethod(nameof(KBArticleAppService.GetBySlugAsync))!
            .GetCustomAttribute<AllowAnonymousAttribute>()
            .ShouldNotBeNull();

        typeof(KBArticleAppService)
            .GetMethod(nameof(KBArticleAppService.IncrementViewCountAsync))!
            .GetCustomAttribute<AllowAnonymousAttribute>()
            .ShouldNotBeNull();
    }

    [Fact]
    public void Update_Input_Should_Allow_Optional_Slug_With_Length_Validation()
    {
        var slugProperty = typeof(UpdateKBArticleDto).GetProperty(nameof(UpdateKBArticleDto.Slug));

        slugProperty.ShouldNotBeNull();
        slugProperty.GetCustomAttribute<RequiredAttribute>().ShouldBeNull();
        slugProperty.GetCustomAttribute<StringLengthAttribute>()!.MaximumLength
            .ShouldBe(KnowledgeBaseConsts.MaxArticleSlugLength);
    }

    [Fact]
    public void Version_Not_Found_Error_Should_Not_ReUse_Slug_Error_Code()
    {
        KnowledgeBaseErrorCodes.ArticleVersionNotFound.ShouldBe($"{KnowledgeBaseErrorCodes.Namespace}:ArticleVersionNotFound");
        KnowledgeBaseErrorCodes.ArticleVersionNotFound.ShouldNotBe(KnowledgeBaseErrorCodes.InvalidArticleSlug);
    }

    [Fact]
    public void Version_Restore_Should_Require_Edit_Permission()
    {
        var authorizeAttribute = typeof(KBArticleVersionAppService)
            .GetMethod(nameof(KBArticleVersionAppService.RestoreAsync))!
            .GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute.Policy.ShouldBe(KnowledgeBasePermissions.Articles.Edit);
    }
}
