using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.FileManager.EntityFrameworkCore;

[ConnectionStringName(SufiAbpFileManagerDbProperties.ConnectionStringName)]
public interface ISufiAbpFileManagerDbContext : IEfCoreDbContext
{
    DbSet<FileItem> FileItems { get; }
    DbSet<FileStructure> FileStructures { get; }
    DbSet<FileFolder> FileFolders { get; }
    DbSet<FolderPermission> FolderPermissions { get; }
}
