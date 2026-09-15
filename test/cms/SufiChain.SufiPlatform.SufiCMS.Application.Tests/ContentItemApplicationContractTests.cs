using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Shouldly;
using SufiChain.SufiPlatform.SufiCMS.Content;
using SufiChain.SufiPlatform.SufiCMS.Permissions;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

public class ContentItemApplicationContractTests
{
    [Fact]
    public void Should_Require_Default_Content_Permission_On_Service()
    {
        var authorizeAttribute = typeof(ContentItemAppService).GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute.Policy.ShouldBe(CMSPermissions.Content.Default);
    }

    [Theory]
    [InlineData(nameof(ContentItemAppService.CreateAsync), CMSPermissions.Content.Create)]
    [InlineData(nameof(ContentItemAppService.UpdateAsync), CMSPermissions.Content.Edit)]
    [InlineData(nameof(ContentItemAppService.DeleteAsync), CMSPermissions.Content.Delete)]
    [InlineData(nameof(ContentItemAppService.UpsertVariantAsync), CMSPermissions.Content.Edit)]
    [InlineData(nameof(ContentItemAppService.PublishVariantAsync), CMSPermissions.Content.Publish)]
    [InlineData(nameof(ContentItemAppService.UnpublishAsync), CMSPermissions.Content.Publish)]
    public void Should_Protect_Mutating_ContentItem_Methods_With_Expected_Policies(string methodName, string policy)
    {
        var method = typeof(ContentItemAppService)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(x => x.Name == methodName);

        var authorizeAttribute = method.GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute.Policy.ShouldBe(policy);
    }

    [Fact]
    public void Public_Content_Services_Should_Be_Anonymous()
    {
        typeof(PublicContentAppService)
            .GetCustomAttribute<AllowAnonymousAttribute>()
            .ShouldNotBeNull();

        typeof(PublicBlogAppService)
            .GetCustomAttribute<AllowAnonymousAttribute>()
            .ShouldNotBeNull();
    }

    [Fact]
    public void ContentItem_Slug_Error_Should_Not_ReUse_NotFound_Code()
    {
        CMSErrorCodes.ContentItemSlugAlreadyExists.ShouldBe($"{CMSErrorCodes.Namespace}:ContentItemSlugAlreadyExists");
        CMSErrorCodes.ContentItemSlugAlreadyExists.ShouldNotBe(CMSErrorCodes.ContentItemNotFound);
    }
}
