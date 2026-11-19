using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.ShortLinks.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Testing;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortLinkTenantRedirectTests : AbpIntegratedTest<ShortLinkTenantRedirectTestModule>
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    [Fact]
    public async Task Create_resolve_redirect_and_click_count_for_host_and_separate_databases()
    {
        await EnsureSchemaAsync(ShortLinkTenantRedirectTestModule.HostDbTenantId);
        await EnsureSchemaAsync(ShortLinkTenantRedirectTestModule.SeparateDbTenantId);

        var hostCode = await InsertAsync(ShortLinkTenantRedirectTestModule.HostDbTenantId, "hostcode", "https://demo.example/home");
        var separateCode = await InsertAsync(ShortLinkTenantRedirectTestModule.SeparateDbTenantId, "dbcode", "https://shop.example/home");

        var hostRedirect = await RedirectAsync(ShortLinkTenantRedirectTestModule.HostDbTenantId, hostCode);
        hostRedirect.ShouldNotBeNull();
        hostRedirect!.Destination.ShouldBe("https://demo.example/home");
        (await ClickCountAsync(ShortLinkTenantRedirectTestModule.HostDbTenantId, hostCode)).ShouldBe(1);
        (await RedirectAsync(ShortLinkTenantRedirectTestModule.HostDbTenantId, separateCode)).ShouldBeNull();

        var separateRedirect = await RedirectAsync(ShortLinkTenantRedirectTestModule.SeparateDbTenantId, separateCode);
        separateRedirect.ShouldNotBeNull();
        separateRedirect!.Destination.ShouldBe("https://shop.example/home");
        (await ClickCountAsync(ShortLinkTenantRedirectTestModule.SeparateDbTenantId, separateCode)).ShouldBe(1);
        (await RedirectAsync(ShortLinkTenantRedirectTestModule.SeparateDbTenantId, hostCode)).ShouldBeNull();
        (await ClickCountAsync(ShortLinkTenantRedirectTestModule.HostDbTenantId, hostCode)).ShouldBe(1);
    }

    private async Task EnsureSchemaAsync(Guid tenantId)
    {
        var current = GetRequiredService<ICurrentTenant>();
        var uow = GetRequiredService<IUnitOfWorkManager>();
        using (current.Change(tenantId))
        using (var unit = uow.Begin(requiresNew: true, isTransactional: false))
        {
            var database = GetRequiredService<ShortLinksDbContext>().Database;
            await database.EnsureCreatedAsync();
            await database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
            await unit.CompleteAsync();
        }
    }

    private async Task<string> InsertAsync(Guid tenantId, string code, string destination)
    {
        var current = GetRequiredService<ICurrentTenant>();
        var uow = GetRequiredService<IUnitOfWorkManager>();
        using (current.Change(tenantId))
        using (var unit = uow.Begin(requiresNew: true, isTransactional: false))
        {
            var link = new ShortUrl(Guid.NewGuid(), code, destination, "Manual")
            {
                TenantId = tenantId
            };
            await GetRequiredService<IShortUrlRepository>().InsertAsync(link, autoSave: true);
            await unit.CompleteAsync();
        }

        return code;
    }

    private async Task<ShortUrlPublicRedirect?> RedirectAsync(Guid tenantId, string code)
    {
        var current = GetRequiredService<ICurrentTenant>();
        var uow = GetRequiredService<IUnitOfWorkManager>();
        using (current.Change(tenantId))
        using (var unit = uow.Begin(requiresNew: true, isTransactional: false))
        {
            var decision = await GetRequiredService<IShortUrlPublicRedirector>().TryRedirectAsync(code, null, "test", "127.0.0.1", null);
            await unit.CompleteAsync();
            return decision;
        }
    }

    private async Task<int> ClickCountAsync(Guid tenantId, string code)
    {
        var current = GetRequiredService<ICurrentTenant>();
        var uow = GetRequiredService<IUnitOfWorkManager>();
        using (current.Change(tenantId))
        using (var unit = uow.Begin(requiresNew: true, isTransactional: false))
        {
            var link = await GetRequiredService<IShortUrlRepository>().FindByShortCodeAsync(code);
            await unit.CompleteAsync();
            return link?.ClickCount ?? -1;
        }
    }
}

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(SufiShortLinksEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule))]
public class ShortLinkTenantRedirectTestModule : AbpModule
{
    public static readonly Guid HostDbTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid SeparateDbTenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly string Root = Path.Combine(Path.GetTempPath(), "sufi-short-links-" + Guid.NewGuid().ToString("N"));
    private static readonly string HostFile = Path.Combine(Root, "host.db");
    private static readonly string SeparateFile = Path.Combine(Root, "separate.db");

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Directory.CreateDirectory(Root);
        var hostConnection = "Data Source=" + HostFile + ";Cache=Shared;Default Timeout=30";
        var separateConnection = "Data Source=" + SeparateFile + ";Cache=Shared;Default Timeout=30";
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = hostConnection;
            options.ConnectionStrings[SufiShortLinksDbProperties.ConnectionStringName] = hostConnection;
        });
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(configuration =>
                configuration.DbContextOptions.UseSqlite(configuration.ConnectionString));
        });
        Configure<AbpMultiTenancyOptions>(options => options.IsEnabled = true);

        var tenants = new Dictionary<Guid, TenantConfiguration>
        {
            [HostDbTenantId] = new TenantConfiguration(HostDbTenantId, "hostdb")
            {
                ConnectionStrings = new ConnectionStrings()
            },
            [SeparateDbTenantId] = new TenantConfiguration(SeparateDbTenantId, "separatedb")
            {
                ConnectionStrings = new ConnectionStrings
                {
                    [ConnectionStrings.DefaultConnectionStringName] = separateConnection
                }
            }
        };
        context.Services.AddTransient<ISufiShortLinksDbContext>(sp => sp.GetRequiredService<ShortLinksDbContext>());
        context.Services.Replace(ServiceDescriptor.Singleton<ITenantStore>(new FixedTenantStore(tenants)));
        context.Services.Replace(ServiceDescriptor.Transient<IConnectionStringResolver, SufiMultiTenantConnectionStringResolver>());
        context.Services.Replace(ServiceDescriptor.Singleton<IFeatureChecker>(new EnabledFeatureChecker()));
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

    private sealed class EnabledFeatureChecker : IFeatureChecker
    {
        public Task<string> GetOrNullAsync(string name) => Task.FromResult("true");

        public Task<bool> IsEnabledAsync(string name) => Task.FromResult(true);
    }

    private sealed class FixedTenantStore : ITenantStore
    {
        private readonly Dictionary<Guid, TenantConfiguration> _tenants;

        public FixedTenantStore(Dictionary<Guid, TenantConfiguration> tenants)
        {
            _tenants = tenants;
        }

        public TenantConfiguration? Find(Guid id) => _tenants.GetValueOrDefault(id);

        public TenantConfiguration? Find(string normalizedName) =>
            _tenants.Values.FirstOrDefault(tenant => tenant.NormalizedName == normalizedName);

        public Task<TenantConfiguration?> FindAsync(Guid id) => Task.FromResult(Find(id));

        public Task<TenantConfiguration?> FindAsync(string normalizedName) => Task.FromResult(Find(normalizedName));

        public Task<IReadOnlyList<TenantConfiguration>> GetListAsync(bool includeDetails = false) =>
            Task.FromResult<IReadOnlyList<TenantConfiguration>>(_tenants.Values.ToList());
    }
}
