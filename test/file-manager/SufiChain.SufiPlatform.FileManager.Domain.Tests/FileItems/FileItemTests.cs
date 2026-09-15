using System;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

public class FileItemTests : FileManagerDomainTestBase<SufiFileManagerDomainTestModule>
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
    public void Should_Set_Source_Entity_And_Custom_Metadata()
    {
        var fileItem = CreateFileItem();
        var sourceEntityId = Guid.NewGuid();

        fileItem.SetSourceEntity(sourceEntityId);
        fileItem.SetCustomMetadata("{\"Capability\":\"Vision\"}");

        fileItem.SourceEntityId.ShouldBe(sourceEntityId);
        fileItem.CustomMetadata.ShouldBe("{\"Capability\":\"Vision\"}");
    }

    [Fact]
    public void Should_Clear_Source_Entity_When_Null_Is_Set()
    {
        var fileItem = CreateFileItem();
        fileItem.SetSourceEntity(Guid.NewGuid());

        fileItem.SetSourceEntity(null);

        fileItem.SourceEntityId.ShouldBeNull();
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