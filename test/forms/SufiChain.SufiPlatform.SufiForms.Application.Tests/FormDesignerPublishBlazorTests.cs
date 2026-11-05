using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

/// <summary>
/// Source-markup proof that the designer toolbar publishes through the existing
/// application service and hides the control without SufiForms.Forms.Publish.
/// bUnit is not used: the page derives from SufiComponentBase.
/// </summary>
public class FormDesignerPublishBlazorTests
{
    private const string Designer =
        "pro-modules/forms/src/SufiChain.SufiPlatform.SufiForms.Blazor/Pages/FormDesignerPage.razor";

    private const string DesignerCode =
        "pro-modules/forms/src/SufiChain.SufiPlatform.SufiForms.Blazor/Pages/FormDesignerPage.razor.cs";

    [Fact]
    public void Designer_toolbar_publishes_and_unpublishes_when_the_permission_is_granted()
    {
        var markup = Normalized(Read(Designer));

        markup.ShouldContain("HasPublishPermission && Definition.PublishState == FormPublishState.Draft");
        markup.ShouldContain("OnClick=\"@PublishAsync\"");
        markup.ShouldContain("L[\"Forms.Publish\"]");
        markup.ShouldContain("HasPublishPermission && Definition.PublishState == FormPublishState.Published");
        markup.ShouldContain("OnClick=\"@UnpublishAsync\"");
        markup.ShouldContain("L[\"Forms.Unpublish\"]");
        markup.ShouldContain("L[$\"Forms.PublishState.{Definition.PublishState}\"]");

        var code = Read(DesignerCode);
        code.ShouldContain("AuthorizationService.IsGrantedAsync(SufiFormsPermissions.Forms.Publish)");
        code.ShouldContain("if (Definition == null || !HasPublishPermission || IsPublishing)");
        code.ShouldContain("FormDefinitionAppService.PublishAsync(DefinitionId)");
        code.ShouldContain("FormDefinitionAppService.UnpublishAsync(DefinitionId)");
        code.ShouldContain("L[\"Forms.PublishedSuccessfully\"]");
        code.ShouldContain("L[\"Forms.UnpublishedSuccessfully\"]");
    }

    [Theory]
    [InlineData("en", "\"Forms.Publish\": \"Publish\"", "\"Forms.Unpublish\": \"Unpublish\"", "\"SufiForms:FormDefinitionNotPublished\": \"The form definition is not published.\"")]
    [InlineData("fa", "\"Forms.Publish\": \"انتشار\"", "\"Forms.Unpublish\": \"لغو انتشار\"", "\"SufiForms:FormDefinitionNotPublished\": \"تعریف فرم منتشر نشده است.\"")]
    public void Designer_publish_strings_exist_for_english_and_persian(string culture, string publish, string unpublish, string blocked)
    {
        var json = Read($"pro-modules/forms/src/SufiChain.SufiPlatform.SufiForms.Domain.Shared/Localization/SufiForms/{culture}.json");
        json.ShouldContain(publish);
        json.ShouldContain(unpublish);
        json.ShouldContain("\"Forms.PublishState.Draft\"");
        json.ShouldContain("\"Forms.PublishState.Published\"");
        json.ShouldContain("\"Forms.PublishedSuccessfully\"");
        json.ShouldContain("\"Forms.UnpublishedSuccessfully\"");
        json.ShouldContain(blocked);
    }

    private static string Read(string relative)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relative);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }
        }

        throw new FileNotFoundException(relative);
    }

    private static string Normalized(string source) => Regex.Replace(source, "\\s+", " ");
}
