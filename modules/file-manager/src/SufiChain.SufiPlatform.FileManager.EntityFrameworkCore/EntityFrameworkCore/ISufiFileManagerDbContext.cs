using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;

[ConnectionStringName(SufiFileManagerDbProperties.ConnectionStringName)]
public interface ISufiFileManagerDbContext : IEfCoreDbContext
{
    DbSet<FileItem> FileItems { get; }
    DbSet<FileStructure> FileStructures { get; }
    DbSet<FileFolder> FileFolders { get; }
    DbSet<FolderPermission> FolderPermissions { get; }
}