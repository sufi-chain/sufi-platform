using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.FileManager.MongoDB;

public static class FileManagerMongoDbContextExtensions
{
    public static void ConfigureSufiAbpFileManager(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<FileItem>(b =>
        {
            b.CollectionName = SufiAbpFileManagerDbProperties.DbTablePrefix + "FileItems";
        });

        builder.Entity<FileFolder>(b =>
        {
            b.CollectionName = SufiAbpFileManagerDbProperties.DbTablePrefix + "FileFolders";
        });

        builder.Entity<FileStructure>(b =>
        {
            b.CollectionName = SufiAbpFileManagerDbProperties.DbTablePrefix + "FileStructures";
        });
    }
}

