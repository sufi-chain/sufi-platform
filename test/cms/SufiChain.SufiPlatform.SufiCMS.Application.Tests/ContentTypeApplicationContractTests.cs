using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Shouldly;
using SufiChain.SufiPlatform.SufiCMS.Content;
using SufiChain.SufiPlatform.SufiCMS.Permissions;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

public class ContentTypeApplicationContractTests
{
    [Fact]
    public void Should_Require_Default_ContentTypes_Permission_On_Service()
    {
        var authorizeAttribute = typeof(ContentTypeAppService).GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute.Policy.ShouldBe(CMSPermissions.ContentTypes.Default);
    }

    [Theory]
    [InlineData(nameof(ContentTypeAppService.CreateAsync), CMSPermissions.ContentTypes.Create)]
    [InlineData(nameof(ContentTypeAppService.UpdateAsync), CMSPermissions.ContentTypes.Edit)]
    [InlineData(nameof(ContentTypeAppService.DeleteAsync), CMSPermissions.ContentTypes.Delete)]
    public void Should_Protect_Mutating_ContentType_Methods_With_Expected_Policies(string methodName, string policy)
    {
        var method = typeof(ContentTypeAppService)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(x => x.Name == methodName);

        var authorizeAttribute = method.GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute.Policy.ShouldBe(policy);
    }

    [Fact]
    public void ContentType_Error_Codes_Should_Be_Distinct()
    {
        CMSErrorCodes.ContentTypeNotFound.ShouldBe($"{CMSErrorCodes.Namespace}:ContentTypeNotFound");
        CMSErrorCodes.ContentTypeNotFound.ShouldNotBe(CMSErrorCodes.ContentTypeAlreadyExists);
        CMSErrorCodes.SystemContentTypeCannotBeDeleted.ShouldNotBe(CMSErrorCodes.ContentTypeNotFound);
    }
}
