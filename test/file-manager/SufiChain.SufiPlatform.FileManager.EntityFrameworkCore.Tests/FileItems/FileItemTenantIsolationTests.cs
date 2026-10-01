using Shouldly;
using SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

public class FileItemTenantIsolationTests : FileManagerTestBase<FileManagerSqliteTestModule>
{
    [Fact]
    public async Task Should_Store_File_For_Owner_And_Hide_It_From_Another_Tenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        using (GetRequiredService<ICurrentTenant>().Change(tenantA))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await GetRequiredService<IFileItemRepository>().InsertAsync(
                    new FileItem(fileId, tenantA, "photo", "photo.png", "blob-1", "image/png", 12, FileType.Image),
                    autoSave: true);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var loaded = await GetRequiredService<IFileItemRepository>().GetAsync(fileId);
                loaded.TenantId.ShouldBe(tenantA);
                loaded.MimeType.ShouldBe("image/png");
            });
        }

        using (GetRequiredService<ICurrentTenant>().Change(tenantB))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                (await GetRequiredService<IFileItemRepository>().FindAsync(fileId)).ShouldBeNull();
            });
        }

        using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
        {
            await WithUnitOfWorkAsync(async () =>
            {
                (await GetRequiredService<IFileItemRepository>().FindAsync(fileId)).ShouldNotBeNull();
            });
        }
    }
}
