using MongoDB.Driver;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.FileManager.MongoDB;

[ConnectionStringName(SufiAbpFileManagerDbProperties.ConnectionStringName)]
public interface IFileManagerMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<FileItem> FileItems { get; }
    IMongoCollection<FileFolder> FileFolders { get; }
    IMongoCollection<FileStructure> FileStructures { get; }
}

