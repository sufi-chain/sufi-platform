using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.BlobDatabase;
using SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.Controllers;
using SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using Volo.Abp.Guids;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager;

/// <summary>
/// Host <c>SufiFileManager</c> points at one catalog. A tenant that only has Default
/// must still read and write File Manager in its own catalog, including when something
/// in the same scope already opened the host catalog.
/// </summary>
public class TenantOwnedDatabaseUploadTests : FileManagerTestBase<TenantOwnedDatabaseUploadModule>
{
    private const string CmsMediaStructureKey = "cms-media";

    private readonly TenantUploadDatabasePaths _paths;

    public TenantOwnedDatabaseUploadTests()
    {
        _paths = GetRequiredService<TenantUploadDatabasePaths>();
    }

    [Fact]
    public async Task Rest_Upload_For_Tenant_With_Only_Default_Lands_In_Tenant_Database()
    {
        var uploaded = await UploadAsync(_paths.OwnedTenantId, "owned.txt", openHostContextFirst: false);

        CountRows(_paths.TenantConnectionString, "FileItems").ShouldBe(1);
        CountRows(_paths.HostConnectionString, "FileItems").ShouldBe(0);
        (await ListFolderAsync(_paths.OwnedTenantId, uploaded.FolderId)).ShouldContain(file => file.Id == uploaded.Id);
    }

    [Fact]
    public async Task Rest_Upload_Stays_On_Tenant_Database_When_Host_Context_Was_Already_Opened()
    {
        var uploaded = await UploadAsync(_paths.OwnedTenantId, "opened-host-first.txt", openHostContextFirst: true);

        CountRows(_paths.TenantConnectionString, "FileItems").ShouldBe(1);
        CountRows(_paths.HostConnectionString, "FileItems").ShouldBe(0);
        (await ListFolderAsync(_paths.OwnedTenantId, uploaded.FolderId)).ShouldContain(file => file.Id == uploaded.Id);
    }

    [Fact]
    public async Task Rest_Upload_For_Tenant_Without_A_Database_Stays_On_Host()
    {
        var uploaded = await UploadAsync(_paths.SharedTenantId, "shared.txt", openHostContextFirst: false);

        CountRows(_paths.HostConnectionString, "FileItems").ShouldBe(1);
        CountRows(_paths.TenantConnectionString, "FileItems").ShouldBe(0);
        (await ListFolderAsync(_paths.SharedTenantId, uploaded.FolderId)).ShouldContain(file => file.Id == uploaded.Id);
    }

    [Fact]
    public async Task Signed_In_Tenant_User_Ignores_Mismatched_Tenant_Header()
    {
        var resolvers = GetRequiredService<IOptions<AbpTenantResolveOptions>>().Value.TenantResolvers;
        resolvers.Select(resolver => resolver.Name).ShouldBe(new[] { "CurrentUser", "Header" });

        var resolved = await ResolveAsSignedInUserAsync(_paths.OwnedTenantId, headerTenantId: _paths.SharedTenantId);
        resolved.AppliedResolvers.ShouldBe(new[] { "CurrentUser" });
        Guid.TryParse(resolved.TenantIdOrName, out var tenantId).ShouldBeTrue();
        tenantId.ShouldBe(_paths.OwnedTenantId);

        var before = CountRows(_paths.TenantConnectionString, "FileItems");
        var uploaded = await UploadAsync(tenantId, "mismatched-header.txt", openHostContextFirst: false);

        CountRows(_paths.TenantConnectionString, "FileItems").ShouldBe(before + 1);
        CountRows(_paths.HostConnectionString, "FileItems").ShouldBe(0);
        (await ListFolderAsync(_paths.OwnedTenantId, uploaded.FolderId)).ShouldContain(file => file.Id == uploaded.Id);
    }

    /// <summary>
    /// CurrentUser is registered ahead of the <c>__tenant</c> header, and it sets Handled for an
    /// authenticated user. The header names a different tenant that shares the host catalog.
    /// </summary>
    private async Task<TenantResolveResult> ResolveAsSignedInUserAsync(Guid signedInTenantId, Guid headerTenantId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(AbpClaimTypes.UserId, Guid.NewGuid().ToString()),
                new Claim(AbpClaimTypes.TenantId, signedInTenantId.ToString())
            },
            authenticationType: "test"));
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["__tenant"] = headerTenantId.ToString();
        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
        try
        {
            using (GetRequiredService<ICurrentPrincipalAccessor>().Change(principal))
            {
                return await GetRequiredService<ITenantResolver>().ResolveTenantIdOrNameAsync();
            }
        }
        finally
        {
            httpContextAccessor.HttpContext = null;
        }
    }

    private async Task<FileItemDto> UploadAsync(Guid tenantId, string fileName, bool openHostContextFirst)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true);

        if (openHostContextFirst)
        {
            var hostContext = scope.ServiceProvider.GetRequiredService<ISufiFileManagerDbContext>();
            _ = await hostContext.FileItems.AnyAsync();
        }

        FileItemDto uploaded;
        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
        using (currentTenant.Change(tenantId))
        {
            await EnsureCmsMediaStructureAsync(scope.ServiceProvider);
            var controller = ActivatorUtilities.CreateInstance<FileItemController>(scope.ServiceProvider);
            await using var request = CreateRequest(fileName);
            var result = await controller.UploadFromFormAsync(request.Form);
            uploaded = ReadUploaded(result);
        }

        await unitOfWork.CompleteAsync();
        return uploaded;
    }

    private async Task<List<FileItemDto>> ListFolderAsync(Guid tenantId, Guid? folderId)
    {
        folderId.ShouldNotBeNull();
        await using var scope = ServiceProvider.CreateAsyncScope();
        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
        using (currentTenant.Change(tenantId))
        {
            var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true);
            var contents = await scope.ServiceProvider.GetRequiredService<IFolderAppService>()
                .GetContentsAsync(new GetFolderContentsInput { FolderId = folderId });
            await unitOfWork.CompleteAsync();
            return contents.Files;
        }
    }

    /// <summary>
    /// Folder listing treats the first path segment as a structure key. The structure row has to
    /// live in the same catalog the upload writes, which for an owned tenant is not the host catalog
    /// the startup seed uses.
    /// </summary>
    private static async Task EnsureCmsMediaStructureAsync(IServiceProvider serviceProvider)
    {
        var structures = serviceProvider.GetRequiredService<IFileStructureRepository>();
        if (await structures.FindByKeyAsync(CmsMediaStructureKey) != null)
        {
            return;
        }

        await structures.InsertAsync(new FileStructure(
            serviceProvider.GetRequiredService<IGuidGenerator>().Create(),
            CmsMediaStructureKey,
            "CMS media",
            FileType.Document,
            "txt",
            "text/plain",
            10 * 1024 * 1024), autoSave: true);
    }

    private static FileItemDto ReadUploaded(IActionResult result)
    {
        if (result is JsonResult json && json.Value is FileItemDto dto)
        {
            return dto;
        }

        var body = result is ObjectResult objectResult ? objectResult.Value : null;
        throw new ShouldAssertException(
            $"Upload did not return a file. Result {result.GetType().Name}, body {body}.");
    }

    private static int CountRows(string connectionString, string tableSuffix)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var find = connection.CreateCommand();
        find.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name LIKE $name";
        find.Parameters.AddWithValue("$name", "%" + tableSuffix);
        var name = find.ExecuteScalar() as string;
        if (name == null)
        {
            return 0;
        }

        using var count = connection.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM \"" + name.Replace("\"", "\"\"") + "\"";
        return Convert.ToInt32(count.ExecuteScalar());
    }

    private static UploadRequest CreateRequest(string fileName)
    {
        var bytes = "tenant-upload"u8.ToArray();
        var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "File", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };
        return new UploadRequest(stream, new UploadFileFormRequest
        {
            File = file,
            StructureKey = CmsMediaStructureKey,
            FolderPath = "/cms-media/landing",
            AutoConfirm = true
        });
    }

    private sealed class UploadRequest : IAsyncDisposable
    {
        private readonly MemoryStream _stream;

        public UploadRequest(MemoryStream stream, UploadFileFormRequest form)
        {
            _stream = stream;
            Form = form;
        }

        public UploadFileFormRequest Form { get; }

        public ValueTask DisposeAsync()
        {
            return _stream.DisposeAsync();
        }
    }
}

public sealed class TenantUploadDatabasePaths
{
    public Guid OwnedTenantId { get; init; }
    public Guid SharedTenantId { get; init; }
    public string HostConnectionString { get; init; } = "";
    public string TenantConnectionString { get; init; } = "";
    public string Directory { get; init; } = "";
}

[DependsOn(
    typeof(SufiFileManagerApplicationModule),
    typeof(SufiFileManagerDomainTestModule),
    typeof(SufiFileManagerEntityFrameworkCoreModule),
    typeof(SufiTenantsDomainModule),
    typeof(AbpEntityFrameworkCoreSqliteModule))]
public class TenantOwnedDatabaseUploadModule : AbpModule
{
    private string _directory = "";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        _directory = Path.Combine(Path.GetTempPath(), "sufi-fm-tenant-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        var hostPath = Path.Combine(_directory, "host.db");
        var tenantPath = Path.Combine(_directory, "tenant.db");
        var hostConnection = "Data Source=" + hostPath;
        var tenantConnection = "Data Source=" + tenantPath;
        CreateTables(hostConnection);
        CreateTables(tenantConnection);

        var paths = new TenantUploadDatabasePaths
        {
            OwnedTenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            SharedTenantId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            HostConnectionString = hostConnection,
            TenantConnectionString = tenantConnection,
            Directory = _directory
        };
        context.Services.AddSingleton(paths);

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = hostConnection;
            options.ConnectionStrings[SufiFileManagerDbProperties.ConnectionStringName] = hostConnection;
            options.ConnectionStrings[SufiBlobDatabaseDbProperties.ConnectionStringName] = hostConnection;
        });
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(configuration =>
                configuration.DbContextOptions.UseSqlite(configuration.ConnectionString));
        });

        context.Services.Replace(ServiceDescriptor.Singleton<ILocalizationTextSeeder, NullLocalizationTextSeeder>());
        context.Services.AddHttpContextAccessor();
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Transient<ITenantStore, UploadTenantStore>());
        // Domain contributors miss an API host. CurrentUser then stops the pipeline, so a
        // mismatched __tenant header cannot select another catalog for a signed-in user.
        Configure<AbpTenantResolveOptions>(options =>
        {
            options.TenantResolvers.Clear();
            options.TenantResolvers.Add(new CurrentUserTenantResolveContributor());
            options.TenantResolvers.Add(new HeaderTenantResolveContributor());
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        try
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }
        catch (IOException)
        {
        }
    }

    private static void CreateTables(string connectionString)
    {
        CreateTables<FileManagerDbContext>(connectionString);
        CreateTables<SufiBlobDatabaseDbContext>(connectionString);
    }

    private static void CreateTables<TContext>(string connectionString)
        where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlite(connectionString)
            .Options;
        using var dbContext = (TContext)Activator.CreateInstance(typeof(TContext), options)!;
        dbContext.GetService<IRelationalDatabaseCreator>().CreateTables();
    }
}

public class UploadTenantStore : ITenantStore
{
    private readonly TenantUploadDatabasePaths _paths;

    public UploadTenantStore(TenantUploadDatabasePaths paths)
    {
        _paths = paths;
    }

    public Task<TenantConfiguration?> FindAsync(string normalizedName)
    {
        return Task.FromResult(Find(normalizedName));
    }

    public Task<TenantConfiguration?> FindAsync(Guid id)
    {
        return Task.FromResult(Find(id));
    }

    public Task<IReadOnlyList<TenantConfiguration>> GetListAsync(bool includeDetails = false)
    {
        IReadOnlyList<TenantConfiguration> list = new[]
        {
            Find(_paths.OwnedTenantId)!,
            Find(_paths.SharedTenantId)!
        };
        return Task.FromResult(list);
    }

    public TenantConfiguration? Find(string normalizedName)
    {
        if (string.Equals(normalizedName, "OWNED", StringComparison.OrdinalIgnoreCase))
        {
            return Find(_paths.OwnedTenantId);
        }

        if (string.Equals(normalizedName, "SHARED", StringComparison.OrdinalIgnoreCase))
        {
            return Find(_paths.SharedTenantId);
        }

        return null;
    }

    public TenantConfiguration? Find(Guid id)
    {
        if (id == _paths.OwnedTenantId)
        {
            var owned = new TenantConfiguration(id, "owned", "OWNED");
            owned.ConnectionStrings!.Default = _paths.TenantConnectionString;
            return owned;
        }

        if (id == _paths.SharedTenantId)
        {
            return new TenantConfiguration(id, "shared", "SHARED");
        }

        return null;
    }
}

internal sealed class NullLocalizationTextSeeder : ILocalizationTextSeeder
{
        public Task UpsertAsync(
            string resourceName,
            string key,
            IReadOnlyDictionary<string, string> cultureValues,
            Guid? tenantId = null,
            bool overwriteExisting = false,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task UpsertManyAsync(
            string resourceName,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> keysByCulture,
            Guid? tenantId = null,
            bool overwriteExisting = false,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task ReplaceKeyValuesAsync(
            string resourceName,
            string key,
            IReadOnlyDictionary<string, string> cultureValues,
            Guid? tenantId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<string?> FindValueAsync(
            string resourceName,
            string cultureName,
            string key,
            Guid? tenantId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<string?>(null);
        }
}
