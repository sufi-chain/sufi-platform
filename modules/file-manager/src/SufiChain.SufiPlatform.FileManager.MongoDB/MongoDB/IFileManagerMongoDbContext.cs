using MongoDB.Driver;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.FileManager.MongoDB;

[ConnectionStringName(SufiFileManagerDbProperties.ConnectionStringName)]
public interface IFileManagerMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<FileItem> FileItems { get; }
    IMongoCollection<FileFolder> FileFolders { get; }
    IMongoCollection<FileStructure> FileStructures { get; }
}