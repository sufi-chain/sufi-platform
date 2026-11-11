using System.Reflection;
using Shouldly;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.FileManager.Blazor.Pages;
using SufiChain.SufiPlatform.FileManager.Features;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Tests;

public class FileManagerPageFeatureTests
{
    [Theory]
    [InlineData(typeof(FileStats), SufiFileManagerFeatures.FileItems)]
    [InlineData(typeof(AssetManager), SufiFileManagerFeatures.FileItems)]
    [InlineData(typeof(FolderAccess), SufiFileManagerFeatures.FileItems)]
    [InlineData(typeof(ImageEditor), SufiFileManagerFeatures.FileItems)]
    [InlineData(typeof(SufiChain.SufiPlatform.FileManager.Blazor.Pages.FileStructures), SufiFileManagerFeatures.FileStructures)]
    [InlineData(typeof(FileStructureWizard), SufiFileManagerFeatures.FileStructures)]
    public void Page_requires_the_master_switch_and_its_child_feature(Type page, string childFeature)
    {
        var features = RequiredFeatures(page);

        features.ShouldContain(SufiFileManagerFeatures.Enable);
        features.ShouldContain(childFeature);
    }

    [Fact]
    public void Archived_files_page_requires_archiving()
    {
        var features = RequiredFeatures(typeof(ArchivedFiles));

        features.ShouldContain(SufiFileManagerFeatures.Enable);
        features.ShouldContain(SufiFileManagerFeatures.FileItems);
        features.ShouldContain(SufiFileManagerFeatures.Archiving);
    }

    private static string[] RequiredFeatures(Type page)
    {
        return page.GetCustomAttributes(inherit: true)
            .OfType<RequiresFeatureAttribute>()
            .SelectMany(attribute => attribute.Features)
            .ToArray();
    }
}
