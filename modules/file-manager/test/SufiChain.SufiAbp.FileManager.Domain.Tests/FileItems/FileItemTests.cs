using System;
using Shouldly;
using SufiChain.SufiAbp.FileManager.FileTypes;
using Xunit;

namespace SufiChain.SufiAbp.FileManager.FileItems;

public class FileItemTests : FileManagerDomainTestBase<FileManagerDomainTestModule>
{
    [Fact]
    public void Should_Archive_And_Restore_File()
    {
        var fileItem = CreateFileItem();

        fileItem.Archive("retention policy");

        fileItem.IsArchived.ShouldBeTrue();
        fileItem.ArchivedAt.ShouldNotBeNull();

        fileItem.RestoreFromArchive();

        fileItem.IsArchived.ShouldBeFalse();
        fileItem.ArchivedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Source_Module_Source_Entity_And_Custom_Metadata()
    {
        var fileItem = CreateFileItem();
        var sourceEntityId = Guid.NewGuid();

        fileItem.SetSourceModule(" AIManagement ");
        fileItem.SetSourceEntity(sourceEntityId);
        fileItem.SetCustomMetadata("{\"Capability\":\"Vision\"}");

        fileItem.SourceModule.ShouldBe("AIManagement");
        fileItem.SourceEntityId.ShouldBe(sourceEntityId);
        fileItem.CustomMetadata.ShouldBe("{\"Capability\":\"Vision\"}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Clear_Source_Module_When_Value_Is_Blank(string? sourceModule)
    {
        var fileItem = CreateFileItem();
        fileItem.SetSourceModule("AIManagement");

        fileItem.SetSourceModule(sourceModule);

        fileItem.SourceModule.ShouldBeNull();
    }

    private static FileItem CreateFileItem()
    {
        return new FileItem(
            Guid.NewGuid(),
            null,
            "test.png",
            "test.png",
            "/general/test.png",
            "image/png",
            128,
            FileType.Image);
    }
}
