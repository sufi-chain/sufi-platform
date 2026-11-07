using System.Reflection;
using Bunit;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.Blazor.Components.Upload;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Components.Upload;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Tests;

/// <summary>
/// The CMS media picker stores dotted, comma-separated extensions. FileUploader used to
/// replace those commas with spaces, which is not a valid HTML accept list. Chromium then
/// treats ".jpg .jpeg .png .gif .webp .avif .svg .ico" as one extension, and the Windows
/// file dialog hides request-1.png. Uppercase in the dialog is the shell's label, not the cause.
/// </summary>
public class FileUploaderAcceptTests : BunitContext
{
    private const string CmsMediaExtensions = ".jpg,.jpeg,.png,.gif,.webp,.avif,.svg,.ico";

    [Fact]
    public void FileUploader_RendersCmsMediaExtensionsAsCommaSeparatedDottedAccept()
    {
        var accept = AcceptFromUploader(CmsMediaExtensions);

        accept.ShouldBe(".jpg,.jpeg,.png,.gif,.webp,.avif,.svg,.ico");
        accept.ShouldNotContain(" ");
        accept.ShouldNotContain("*.*");
    }

    [Fact]
    public void FileUploader_DotsBareExtensionsAndKeepsMimeWildcards()
    {
        var accept = AcceptFromUploader(" JPG, JPEG, *.PNG, image/*, image/png, .PNG ");

        accept.ShouldBe(".jpg,.jpeg,.png,image/*,image/png");
    }

    [Fact]
    public void FileUploader_KeepsAvifAndAviAsSeparateTokens()
    {
        AcceptFromUploader("jpg,jpeg,png,gif,webp,avif,avi")
            .ShouldBe(".jpg,.jpeg,.png,.gif,.webp,.avif,.avi");
    }

    [Fact]
    public void FileUploader_WithoutAStructure_StaysUnconstrained()
    {
        AcceptFromUploader(null, hasStructure: false).ShouldBe("*/*");
    }

    [Fact]
    public void FileUploader_EmptyExtensionList_DoesNotBecomeAllFiles()
    {
        AcceptFromUploader(string.Empty).ShouldBe(string.Empty);
        AcceptFromUploader("   ").ShouldBe(string.Empty);
        AcceptFromUploader(null).ShouldBe(string.Empty);
    }

    [Fact]
    public void NativeFileInput_RendersTheUploaderAcceptOnTheDom()
    {
        var accept = AcceptFromUploader(CmsMediaExtensions);
        var cut = Render<NativeFileInput>(parameters => parameters
            .Add(component => component.Id, "cms-media-picker")
            .Add(component => component.CssClass, "d-none")
            .Add(component => component.Multiple, true)
            .Add(component => component.Accept, accept));

        var input = cut.Find("input[type=file]");
        input.GetAttribute("accept").ShouldBe(".jpg,.jpeg,.png,.gif,.webp,.avif,.svg,.ico");
        input.GetAttribute("id").ShouldBe("cms-media-picker");
        input.GetAttribute("multiple").ShouldBe("multiple");
        input.ClassList.ShouldContain("d-none");
    }

    [Fact]
    public void NativeFileInput_RepairsASpaceSeparatedDottedList()
    {
        var cut = Render<NativeFileInput>(parameters => parameters
            .Add(component => component.Accept, ".jpg .jpeg .png .gif .webp .AVIF .svg .ico"));

        cut.Find("input[type=file]").GetAttribute("accept")
            .ShouldBe(".jpg,.jpeg,.png,.gif,.webp,.avif,.svg,.ico");
    }

    [Fact]
    public void NativeFileInput_KeepsAnImageWildcard()
    {
        var cut = Render<NativeFileInput>(parameters => parameters.Add(component => component.Accept, "IMAGE/*"));
        cut.Find("input[type=file]").GetAttribute("accept").ShouldBe("image/*");
    }

    [Fact]
    public void NativeFileInput_KeepsThePublicUploaderDefaultConstraint()
    {
        const string publishedDefault = "image/*,application/pdf,video/*,.docx,.html,.htm,.txt,.md";
        var cut = Render<NativeFileInput>(parameters => parameters
            .Add(component => component.Accept, publishedDefault));

        var accept = cut.Find("input[type=file]").GetAttribute("accept");
        accept.ShouldBe(publishedDefault);
        accept.ShouldNotBeNullOrWhiteSpace();
        accept.ShouldNotContain("*/*");
    }

    [Fact]
    public void FileUploaderMarkup_BindsTheNativeInputToTheNormalizedAccept()
    {
        var markup = Read("modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Blazor/Components/Upload/FileUploader.razor");
        var code = Read("modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Blazor/Components/Upload/FileUploader.razor.cs");
        var publicMarkup = Read("modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Blazor.Public/Components/Upload/PublicFileUploader.razor");
        var quickImage = Read("modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Blazor/Components/Upload/QuickImageUploader.razor");

        markup.ShouldContain("<NativeFileInput");
        markup.ShouldContain("Accept=\"@GetAcceptedFileTypes()\"");
        markup.ShouldNotContain("<input type=\"file\"");
        code.ShouldContain("HtmlFileAccept.Normalize");
        code.ShouldNotContain("Replace(',', ' ')");
        publicMarkup.ShouldContain("<NativeFileInput");
        publicMarkup.ShouldContain("Accept=\"@Accept\"");
        publicMarkup.ShouldNotContain("<input type=\"file\"");
        quickImage.ShouldContain("Accept=\"image/*\"");
        quickImage.ShouldNotContain("<input type=\"file\"");
    }

    private static string AcceptFromUploader(string? allowedExtensions, bool hasStructure = true)
    {
        var uploader = new FileUploader();
        if (hasStructure)
        {
            typeof(FileUploader)
                .GetField("_structure", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(uploader, new FileStructureDto
                {
                    Key = "cms-media",
                    DisplayName = "Website media",
                    AllowedExtensions = allowedExtensions!,
                    AllowedMimeTypes = "image/png"
                });
        }

        var method = typeof(FileUploader).GetMethod(
            "GetAcceptedFileTypes",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return (string)method!.Invoke(uploader, null)!;
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
}
