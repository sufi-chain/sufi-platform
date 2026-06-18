using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager">
using SufiChain.SufiAbp.FileManager;
using SufiChain.SufiAbp.FileManager.Blazor;
using SufiChain.SufiAbp.FileManager.Blazor.Server;
// </TEMPLATE-REMOVE>
using SufiChain.SufiAbp.Identity;
// <TEMPLATE-REMOVE IF-NOT="module:tenant-management">
using SufiChain.SufiAbp.TenantManagement;
// </TEMPLATE-REMOVE>
using SufiChain.SufiAbp.UI.Blazor.Server.MultiTenancy;
using Volo.Abp.AspNetCore.MultiTenancy;
using SufiChain.KomTheme;
using SufiChain.KomTheme.Blazor.Server;
using SufiChain.KomTheme.Blazor.Server.Bundling;
using SufiChain.SufiAbp.UI;
using SufiChain.SufiAbp.UI.Bundling;
using SufiChain.SufiAbp.UI.Routing;
using SufiChain.SufiAbp.UI.Toolbars;
using StackExchange.Redis;
using SufiChain.SufiAbp.AspNetCore.Authentication.OpenIdConnect;
using SufiChain.SufiAbp.AspNetCore.Mvc.Client;
using SufiChain.SufiAbp.AspNetCore.MultiTenancy;
using SufiChain.SufiAbp.AspNetCore.Serilog;
using SufiChain.SufiAbp.Autofac;
using SufiChain.SufiAbp.Caching.StackExchangeRedis;
using SufiChain.SufiAbp.Http.Client.IdentityModel.Web;
using SufiChain.SufiAbp.Swashbuckle;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.Caching;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.VirtualFileSystem;
using MyCompanyName.MyProjectName.Blazor.WebSite.Menus;
using MyCompanyName.MyProjectName.Localization;
using MyCompanyName.MyProjectName.MultiTenancy;

namespace MyCompanyName.MyProjectName.Blazor.WebSite;

/// <summary>
/// ABP Module for DemoApp Blazor WebSite host.
/// This is the public-facing website that will serve CMS content with dynamic layouts.
/// 
/// Key differences from WebApp (Admin):
/// - NO admin modules (AuditLogging, BackgroundJobs, SettingManagement, FeatureManagement)
/// - NO TenantManagement (managed via Admin panel only)
/// - Uses dynamic layout rendering for CMS pages (placeholder for now)
/// - Includes Identity.Public for login/register pages
/// </summary>
[DependsOn(
    typeof(DemoAppHttpApiClientModule),
    typeof(SufiAbpCachingStackExchangeRedisModule),
    typeof(SufiAbpAspNetCoreMvcClientModule),
    typeof(SufiAbpAuthenticationOpenIdConnectModule),
    typeof(SufiAbpHttpClientIdentityModelWebModule),
    typeof(SufiAbpAutofacModule),
    typeof(SufiAbpSwashbuckleModule),
    typeof(SufiAbpAspNetCoreSerilogModule),
    // SufiAbp UI ABP Integration - bridges ABP services (menus, languages, users, etc.) to SufiAbp UI
    typeof(SufiAbpUIModule),
    // SufiAbp UI Modules for Public Site
    // NOTE: SufiAbpAccountBlazorModule is NOT included here because auth pages
    // are served by the dedicated AuthServer host in tiered architecture.
    typeof(SufiAbpIdentityHttpApiClientModule),
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager">
    // File Manager for public file access (e.g., media in CMS pages)
    typeof(SufiAbpFileManagerBlazorModule),
    typeof(SufiAbpFileManagerBlazorServerModule),
    typeof(SufiAbpFileManagerHttpApiClientModule),
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="module:tenant-management">
    // Tenant Management HTTP API Client (provides remote ITenantStore for tenant cookie resolution)
    typeof(SufiAbpTenantManagementHttpApiClientModule),
    // </TEMPLATE-REMOVE>
    // KomTheme using SufiBlazor design system
    // NOTE: For CMS, this will be replaced with dynamic layout rendering
    typeof(KomThemeBlazorServerModule),
    // ABP Multi-Tenancy (cookie/header/domain resolvers + middleware)
    typeof(SufiAbpAspNetCoreMultiTenancyModule)
)]
public class DemoAppBlazorWebSiteModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(DemoAppResource),
                typeof(DemoAppDomainSharedModule).Assembly,
                typeof(DemoAppApplicationContractsModule).Assembly,
                typeof(DemoAppBlazorWebSiteModule).Assembly
            );
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        // Add Blazor Web App services (modern .NET 8+ pattern)
        context.Services.AddRazorComponents()
            .AddInteractiveServerComponents(options =>
            {
                if (hostingEnvironment.IsDevelopment())
                {
                    options.DetailedErrors = true;
                }
            });
        
        // <TEMPLATE-REMOVE IF-NOT="module:file-manager">
        // Configure SignalR for large file uploads
        context.Services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = 100 * 1024 * 1024;
        });
        // </TEMPLATE-REMOVE>

        ConfigureUrls(configuration);
        ConfigureCache();
        ConfigureBundles();
        ConfigureMultiTenancy();
        ConfigureAuthentication(context, configuration);
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureLocalizationServices();
        ConfigureRouter(context);
        ConfigureMenu(configuration);
        ConfigureDataProtection(context, configuration, hostingEnvironment);
        ConfigureSwaggerServices(context.Services);
        
        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        // App URL configuration handled by ABP's Volo.Abp.UI.Navigation.Urls package
    }

    private void ConfigureCache()
    {
        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "DemoApp:WebSite:";
        });
    }

    private void ConfigureBundles()
    {
        // Configure SufiAbp bundling for additional app-specific styles
        Configure<BundleOptions>(options =>
        {
            options.StyleBundles.Add(BlazorKomThemeBundles.Styles.Global, "/blazor-global-styles.css");
        });
    }

    private void ConfigureMultiTenancy()
    {
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies", options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(365);
            })
            .AddAbpOpenIdConnect("oidc", options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

                options.ClientId = configuration["AuthServer:ClientId"];
                options.ClientSecret = configuration["AuthServer:ClientSecret"];

                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.Scope.Add("roles");
                options.Scope.Add("email");
                options.Scope.Add("phone");
                options.Scope.Add("DemoApp");
            });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<DemoAppDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}MyCompanyName.MyProjectName.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<DemoAppApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}MyCompanyName.MyProjectName.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<DemoAppBlazorWebSiteModule>(hostingEnvironment.ContentRootPath);
                // <TEMPLATE-REMOVE>
                options.FileSets.ReplaceEmbeddedByPhysical<KomThemeBlazorServerModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}sufi-theme{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.KomTheme.Blazor.Server"));
                // Load SufiAbpFramework and FileManager localization from source so base-type fallback and fa/ar work in dev
                options.FileSets.ReplaceEmbeddedByPhysical<UiDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, "..", "..", "..", "..", "..", "src", "framework", "src", "SufiChain.SufiAbp.UI.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<FileManagerDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, "..", "..", "..", "..", "..", "src", "modules", "file-manager", "src", "SufiChain.SufiAbp.FileManager.Domain.Shared"));
                // </TEMPLATE-REMOVE>
            });
        }
    }

    private void ConfigureLocalizationServices()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            // Primary languages (en, fa, ar)
            // Note: RTL is determined automatically by the culture (ar, fa are RTL)
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            options.Languages.Add(new LanguageInfo("fa", "fa", "فارسی"));
            options.Languages.Add(new LanguageInfo("ar", "ar", "العربية"));
            options.Languages.Add(new LanguageInfo("es", "es", "Español"));
        });

        Configure<SufiChain.SufiAbp.UI.Localization.SufiAbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new SufiChain.SufiAbp.UI.Localization.LanguageInfo("en", "en", "English"));
            options.Languages.Add(new SufiChain.SufiAbp.UI.Localization.LanguageInfo("fa", "fa", "فارسی", isRtl: true));
            options.Languages.Add(new SufiChain.SufiAbp.UI.Localization.LanguageInfo("ar", "ar", "العربية", isRtl: true));
            options.Languages.Add(new SufiChain.SufiAbp.UI.Localization.LanguageInfo("es", "es", "Español"));
        });
    }

    private void ConfigureMenu(IConfiguration configuration)
    {
        Configure<ToolbarOptions>(options =>
        {
            options.Contributors.Add(new DemoAppPublicToolbarContributor());
        });
    }

    private void ConfigureRouter(ServiceConfigurationContext context)
    {
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AppAssembly = typeof(DemoAppBlazorWebSiteModule).Assembly;
        });
        
        // Configure KomTheme for public site
        // TODO: In future CMS implementation, this will be replaced with dynamic layout selection
        // based on the CMS page being rendered. For now, use a simple sidebar layout.
        Configure<KomThemeBlazorOptions>(options =>
        {
            options.Layout = KomLayouts.TopMenu;
            options.IconRailDarkMode = false;
            options.ExpandOnHover = true;
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "DemoApp WebSite API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }

    private void ConfigureDataProtection(
        ServiceConfigurationContext context,
        IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment)
    {
        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("DemoApp_WebSite");
        if (!hostingEnvironment.IsDevelopment())
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "DemoApp-WebSite-Protection-Keys");
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var env = context.GetEnvironment();
        var app = context.GetApplicationBuilder();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseSpTenantSwitch();
            app.UseMultiTenancy();
        }

        app.UseAuthorization();
        
        // Required for Blazor Web App antiforgery
        app.UseAntiforgery();
        
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "DemoApp WebSite API");
        });
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints(endpoints =>
        {
            var routerOptions = endpoints.ServiceProvider
                .GetRequiredService<IOptions<SufiAbpRouterOptions>>()
                .Value;
            
            endpoints.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddAdditionalAssemblies(routerOptions.AdditionalAssemblies.Distinct().ToArray());
        });
    }
}
