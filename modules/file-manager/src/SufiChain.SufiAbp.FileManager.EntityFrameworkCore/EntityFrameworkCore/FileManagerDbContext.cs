using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.FileManager.EntityFrameworkCore;

[ConnectionStringName(SufiAbpFileManagerDbProperties.ConnectionStringName)]
public class FileManagerDbContext : AbpDbContext<FileManagerDbContext>, ISufiAbpFileManagerDbContext
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
        builder.ConfigureSufiAbpBlobStoringDatabase();
    }
}
