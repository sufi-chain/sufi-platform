using Shouldly;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.Storage;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager;

public class FileItemBlobAccessServiceTests
{
    [Fact]
    public void GetContainerName_Should_Return_Default_When_StructureKey_Is_Null_Or_Empty()
    {
        FileItemBlobAccessService.GetContainerName(null)
            .ShouldBe(FileStructureStorageConstants.DefaultContainerName);

        FileItemBlobAccessService.GetContainerName(string.Empty)
            .ShouldBe(FileStructureStorageConstants.DefaultContainerName);
    }

    [Theory]
    [InlineData("avatars")]
    [InlineData("documents")]
    public void GetContainerName_Should_Prefix_StructureKey(string structureKey)
    {
        FileItemBlobAccessService.GetContainerName(structureKey)
            .ShouldBe(FileStructureStorageConstants.ContainerNamePrefix + structureKey);
    }
}
