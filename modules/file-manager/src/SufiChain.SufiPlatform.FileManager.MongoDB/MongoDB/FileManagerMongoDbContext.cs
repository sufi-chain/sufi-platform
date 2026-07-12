using MongoDB.Driver;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.FileManager.MongoDB;

[ConnectionStringName(SufiFileManagerDbProperties.ConnectionStringName)]
public class FileManagerMongoDbContext : AbpMongoDbContext, IFileManagerMongoDbContext
{
    public IMongoCollection<FileItem> FileItems => Collection<FileItem>();
    public IMongoCollection<FileFolder> FileFolders => Collection<FileFolder>();
    public IMongoCollection<FileStructure> FileStructures => Collection<FileStructure>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);
    }
}