using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.FileManager.MongoDB;

public static class FileManagerMongoDbContextExtensions
{
    public static void ConfigureSufiFileManager(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<FileItem>(b =>
        {
            b.CollectionName = SufiFileManagerDbProperties.DbTablePrefix + "FileItems";
        });

        builder.Entity<FileFolder>(b =>
        {
            b.CollectionName = SufiFileManagerDbProperties.DbTablePrefix + "FileFolders";
        });

        builder.Entity<FileStructure>(b =>
        {
            b.CollectionName = SufiFileManagerDbProperties.DbTablePrefix + "FileStructures";
        });
    }
}