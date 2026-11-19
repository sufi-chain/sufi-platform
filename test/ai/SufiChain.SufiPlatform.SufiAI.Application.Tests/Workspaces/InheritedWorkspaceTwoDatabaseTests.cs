using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.Provider.OpenRouter;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

/// <summary>
/// Host workspace and tenant projection live in different SQLite files.
/// Rotating the host key is visible to the tenant without copying it into the tenant file.
/// </summary>
public class InheritedWorkspaceTwoDatabaseTests : SufiAITestBase<InheritedWorkspaceTwoDatabaseTestModule>
{
    private const string StaleKey = "sk-stale-copy";
    private const string LiveKey = "sk-live-rotated";

    [Fact]
    public async Task Tenant_database_keeps_no_usable_host_key_after_rotation()
    {
        var hostId = Guid.NewGuid();
        var tenantId = InheritedWorkspaceTwoDatabaseTestModule.TenantId;
        var projectionId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var name = "two-db-" + hostId.ToString("N")[..8];
        var workspaces = GetRequiredService<IWorkspaceRepository>();
        var synchronizer = GetRequiredService<IInheritedWorkspaceProjectionSynchronizer>();
        var liveView = GetRequiredService<IInheritedWorkspaceLiveView>();
        var currentTenant = GetRequiredService<ICurrentTenant>();
        var runtime = GetRequiredService<IWorkspaceRuntimeConfigurationResolver>();

        using (currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var host = new Workspace(hostId, name, AIProviderType.OpenRouter, "openrouter/free");
                host.UpdateConfiguration("openrouter/free", "sk-host-original", "https://host.example/v1", 1m, 2m);
                host.AddModelConfiguration(
                    AICapabilityType.ChatCompletion,
                    "openrouter/free",
                    apiEndpoint: "https://host.example/v1",
                    apiKey: "sk-host-original",
                    priority: 0,
                    inputPrice: 10m,
                    outputPrice: 20m,
                    isUserSelectable: true);
                await workspaces.InsertAsync(host, autoSave: true);
            });

            var source = await workspaces.GetAsync(hostId, includeDetails: true);
            await synchronizer.CreateTenantProjectionAsync(
                source,
                tenantId,
                assignmentId,
                projectionId,
                hostId,
                name + "-tenant");
        }

        using (currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                using (liveView.Suppress())
                {
                    var stored = await workspaces.GetAsync(projectionId, includeDetails: true);
                    stored.UpdateConfiguration(stored.DefaultModel, StaleKey, "https://stale.example/v1");
                    foreach (var route in stored.ModelConfigurations)
                    {
                        route.UpdateConfiguration(
                            route.ModelId,
                            "https://stale.example/v1",
                            StaleKey,
                            route.Priority,
                            route.OpenAIApiMode,
                            route.InputPrice,
                            route.OutputPrice,
                            route.Dimensions,
                            route.DisplayName,
                            route.IsUserSelectable,
                            route.Description,
                            route.MaxContextTokens,
                            route.InputPriceUnit,
                            route.OutputPriceUnit);
                    }

                    await workspaces.UpdateAsync(stored, autoSave: true);
                }
            });
        }

        using (currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var host = await workspaces.GetAsync(hostId, includeDetails: true);
                host.UpdateConfiguration("openrouter/free", LiveKey, "https://live.example/v1", 1m, 2m);
                foreach (var route in host.ModelConfigurations)
                {
                    route.UpdateConfiguration(
                        route.ModelId,
                        "https://live.example/v1",
                        LiveKey,
                        route.Priority,
                        route.OpenAIApiMode,
                        route.InputPrice,
                        route.OutputPrice,
                        route.Dimensions,
                        route.DisplayName,
                        route.IsUserSelectable,
                        route.Description,
                        route.MaxContextTokens,
                        route.InputPriceUnit,
                        route.OutputPriceUnit);
                }

                await workspaces.UpdateAsync(host, autoSave: true);
            });
        }

        CountRows(InheritedWorkspaceTwoDatabaseTestModule.HostConnection).ShouldBe(1);
        CountRows(InheritedWorkspaceTwoDatabaseTestModule.TenantConnection).ShouldBe(1);
        ReadKeys(InheritedWorkspaceTwoDatabaseTestModule.HostConnection).ShouldBe(new[] { LiveKey });
        ReadKeys(InheritedWorkspaceTwoDatabaseTestModule.TenantConnection).ShouldBe(new[] { StaleKey });

        using (currentTenant.Change(tenantId))
        {
            var resolved = await runtime.ResolveAsync(projectionId, AICapabilityType.ChatCompletion);
            resolved.ApiKey.ShouldBe(LiveKey);
            resolved.Workspace.TenantId.ShouldBe(tenantId);
            resolved.Workspace.Id.ShouldBe(projectionId);
        }

        using var tenantConnection = new SqliteConnection(InheritedWorkspaceTwoDatabaseTestModule.TenantConnection);
        tenantConnection.Open();
        using (var command = tenantConnection.CreateCommand())
        {
            command.CommandText = InheritedWorkspaceCredentialBlanking.WorkspaceSql;
            command.ExecuteNonQuery();
        }

        using (var command = tenantConnection.CreateCommand())
        {
            command.CommandText = InheritedWorkspaceCredentialBlanking.RouteSql;
            command.ExecuteNonQuery();
        }

        ReadKeys(InheritedWorkspaceTwoDatabaseTestModule.TenantConnection).ShouldBe(new string?[] { null });
        ReadKeys(InheritedWorkspaceTwoDatabaseTestModule.HostConnection).ShouldBe(new[] { LiveKey });
    }

    private static int CountRows(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM [SufiAI.Workspaces]";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static List<string?> ReadKeys(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT [ApiKey] FROM [SufiAI.Workspaces]";
        using var reader = command.ExecuteReader();
        var keys = new List<string?>();
        while (reader.Read())
        {
            keys.Add(reader.IsDBNull(0) ? null : reader.GetString(0));
        }

        return keys;
    }
}

[DependsOn(
    typeof(SufiAITestBaseModule),
    typeof(SufiAIEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule),
    typeof(SufiAIOpenRouterModule),
    typeof(SufiAIApplicationModule))]
public class InheritedWorkspaceTwoDatabaseTestModule : AbpModule
{
    public static readonly Guid TenantId = Guid.Parse("b7c1a6d2-4e18-4c0a-9f31-6a2d0e5b77c4");

    private static readonly string Root = Path.Combine(
        Path.GetTempPath(),
        "l7-inherited-" + Guid.NewGuid().ToString("N"));

    public static string HostConnection { get; } = "Data Source=" + Path.Combine(Root, "host.db");

    public static string TenantConnection { get; } = "Data Source=" + Path.Combine(Root, "tenant.db");

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Directory.CreateDirectory(Root);
        CreateDatabase(HostConnection);
        CreateDatabase(TenantConnection);

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = HostConnection;
            options.ConnectionStrings[SufiAIDbProperties.ConnectionStringName] = HostConnection;
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<AIDbContext>(configuration =>
            {
                configuration.DbContextOptions.UseSqlite(configuration.ConnectionString);
            });
        });

        context.Services.AddHttpClient();
        context.Services.RemoveAll<IModelCatalogProvider>();
        context.Services.AddTransient<IModelCatalogProvider, FakeOpenRouterModelCatalogProvider>();
        context.Services.Replace(
            ServiceDescriptor.Transient<IAIHooshvareModelSelectionPolicyProvider, TestHooshvareModelSelectionPolicyProvider>());
        context.Services.TryAddSingleton(Substitute.For<IBlobContainer>());
        context.Services.Replace(
            ServiceDescriptor.Singleton<IDynamicSettingDefinitionStore, NullDynamicSettingDefinitionStore>());
        context.Services.Replace(ServiceDescriptor.Transient<ISettingStore, NullSettingStore>());
        context.Services.RemoveAll<ITenantStore>();
        context.Services.AddSingleton<ITenantStore, InheritedWorkspaceTwoDatabaseTenantStore>();
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        try
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
        catch (IOException)
        {
        }
    }

    private static void CreateDatabase(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var options = new DbContextOptionsBuilder<AIDbContext>().UseSqlite(connection).Options;
        using var db = new AIDbContext(options);
        db.GetService<IRelationalDatabaseCreator>().CreateTables();
    }
}

internal sealed class InheritedWorkspaceTwoDatabaseTenantStore : ITenantStore
{
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
        IReadOnlyList<TenantConfiguration> list = Find(InheritedWorkspaceTwoDatabaseTestModule.TenantId) is { } tenant
            ? new[] { tenant }
            : Array.Empty<TenantConfiguration>();
        return Task.FromResult(list);
    }

    public TenantConfiguration? Find(string normalizedName)
    {
        return string.Equals(normalizedName, "OWNED", StringComparison.OrdinalIgnoreCase)
            ? Find(InheritedWorkspaceTwoDatabaseTestModule.TenantId)
            : null;
    }

    public TenantConfiguration? Find(Guid id)
    {
        if (id != InheritedWorkspaceTwoDatabaseTestModule.TenantId)
        {
            return null;
        }

        var tenant = new TenantConfiguration(id, "owned", "OWNED");
        tenant.ConnectionStrings!.Default = InheritedWorkspaceTwoDatabaseTestModule.TenantConnection;
        tenant.ConnectionStrings[SufiAIDbProperties.ConnectionStringName] =
            InheritedWorkspaceTwoDatabaseTestModule.TenantConnection;
        return tenant;
    }
}
