using MongoDB.Driver;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.FileManager.MongoDB;

[ConnectionStringName(SufiAbpFileManagerDbProperties.ConnectionStringName)]
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

