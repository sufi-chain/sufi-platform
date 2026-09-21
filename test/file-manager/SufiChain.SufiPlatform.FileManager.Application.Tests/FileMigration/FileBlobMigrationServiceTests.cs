using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileMigration;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp.BlobStoring;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.FileMigration;

public class FileBlobMigrationServiceTests
{
    [Fact]
    public async Task Should_Skip_When_Provider_Already_Matches()
    {
        var item = CreateFile(FileStructureStorageProvider.S3Provider);
        var source = Substitute.For<IBlobContainer>();
        var destination = Substitute.For<IBlobContainer>();
        var containers = Substitute.For<IStructureBlobContainerProvider>();
        containers.GetWriteContainerAsync(item.StructureKey, Arg.Any<CancellationToken>())
            .Returns(new StructureBlobContainerResult(destination, FileStructureStorageProvider.S3Provider));
        containers.GetContainerAsync(item.StructureKey, item.StorageProvider, Arg.Any<CancellationToken>())
            .Returns(source);

        var repository = Substitute.For<IFileItemRepository>();
        var service = new FileBlobMigrationService(containers, repository);

        var migrated = await service.MigrateFileAsync(item);

        migrated.ShouldBeFalse();
        await repository.DidNotReceive().UpdateAsync(item, Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Copy_Verify_Update_Then_Delete_Source()
    {
        var item = CreateFile(FileStructureStorageProvider.Database);
        var source = Substitute.For<IBlobContainer>();
        var destination = Substitute.For<IBlobContainer>();
        source.GetOrNullAsync(item.BlobName, Arg.Any<CancellationToken>())
            .Returns(new MemoryStream(new byte[] { 1, 2, 3 }));
        destination.ExistsAsync(item.BlobName, Arg.Any<CancellationToken>()).Returns(false, true);

        var containers = Substitute.For<IStructureBlobContainerProvider>();
        containers.GetWriteContainerAsync(item.StructureKey, Arg.Any<CancellationToken>())
            .Returns(new StructureBlobContainerResult(destination, FileStructureStorageProvider.S3Provider));
        containers.GetContainerAsync(item.StructureKey, item.StorageProvider, Arg.Any<CancellationToken>())
            .Returns(source);

        var repository = Substitute.For<IFileItemRepository>();
        var service = new FileBlobMigrationService(containers, repository);

        var migrated = await service.MigrateFileAsync(item);

        migrated.ShouldBeTrue();
        item.StorageProvider.ShouldBe(FileStructureStorageProvider.S3Provider);
        await destination.Received().SaveAsync(item.BlobName, Arg.Any<Stream>(), true, Arg.Any<CancellationToken>());
        await repository.Received().UpdateAsync(item, true, Arg.Any<CancellationToken>());
        await source.Received().DeleteAsync(item.BlobName, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Move_File_To_Another_Structure()
    {
        var item = CreateFile(FileStructureStorageProvider.Database);
        var source = Substitute.For<IBlobContainer>();
        var destination = Substitute.For<IBlobContainer>();
        source.GetOrNullAsync(item.BlobName, Arg.Any<CancellationToken>())
            .Returns(new MemoryStream(new byte[] { 9 }));
        destination.ExistsAsync(item.BlobName, Arg.Any<CancellationToken>()).Returns(false, true);

        var containers = Substitute.For<IStructureBlobContainerProvider>();
        containers.GetWriteContainerAsync("target", Arg.Any<CancellationToken>())
            .Returns(new StructureBlobContainerResult(destination, FileStructureStorageProvider.FileSystem));
        containers.GetContainerAsync(item.StructureKey, item.StorageProvider, Arg.Any<CancellationToken>())
            .Returns(source);

        var repository = Substitute.For<IFileItemRepository>();
        var service = new FileBlobMigrationService(containers, repository);
        var folderId = Guid.NewGuid();

        var migrated = await service.MigrateFileAsync(item, "target", folderId);

        migrated.ShouldBeTrue();
        item.StructureKey.ShouldBe("target");
        item.FolderId.ShouldBe(folderId);
        item.StorageProvider.ShouldBe(FileStructureStorageProvider.FileSystem);
    }

    private static FileItem CreateFile(FileStructureStorageProvider provider)
    {
        var item = new FileItem(
            Guid.NewGuid(),
            null,
            "a.png",
            "a.png",
            "2026/09/a.png",
            "image/png",
            3,
            FileType.Image,
            "docs");
        item.IsTemp = false;
        item.SetStorageProvider(provider);
        return item;
    }
}
