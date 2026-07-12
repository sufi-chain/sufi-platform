using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;

[ConnectionStringName(SufiFileManagerDbProperties.ConnectionStringName)]
public class FileManagerDbContext : AbpDbContext<FileManagerDbContext>, ISufiFileManagerDbContext
{
    public DbSet<FileItem> FileItems { get; set; } = null!;
    public DbSet<FileStructure> FileStructures { get; set; } = null!;
    public DbSet<FileFolder> FileFolders { get; set; } = null!;
    public DbSet<FolderPermission> FolderPermissions { get; set; } = null!;

    public FileManagerDbContext(DbContextOptions<FileManagerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureSufiBlobDatabase();
    }
}