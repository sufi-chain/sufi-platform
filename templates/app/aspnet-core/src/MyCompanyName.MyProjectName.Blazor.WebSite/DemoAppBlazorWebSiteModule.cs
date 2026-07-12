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
using Volo.Abp.Http.Client.IdentityModel.Web;
using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.AspNetCore.Mvc.Client;
using Volo.Abp.Swashbuckle;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.UI;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager">
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.FileManager.Blazor;
using SufiChain.SufiPlatform.FileManager.Blazor.Server;
// </TEMPLATE-REMOVE>
using SufiChain.SufiPlatform.Identity;
// <TEMPLATE-REMOVE IF-NOT="module:tenants">
using SufiChain.SufiPlatform.Tenants;
// </TEMPLATE-REMOVE>
using SufiChain.SufiPlatform.UI.Blazor.Server.MultiTenancy;
using Volo.Abp.AspNetCore.MultiTenancy;
using SufiChain.SufiTheme;
using SufiChain.SufiTheme.Blazor.Server;
using SufiChain.SufiTheme.Blazor.Server.Bundling;
using SufiChain.SufiPlatform.UI.Bundling;
using SufiChain.SufiPlatform.UI.Routing;
using SufiChain.SufiPlatform.UI.Toolbars;
using StackExchange.Redis;
using SufiChain.SufiPlatform.AspNetCore.Authentication.OpenIdConnect;
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
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpAspNetCoreMvcClientModule),
    typeof(SufiAuthenticationOpenIdConnectModule),
    typeof(AbpHttpClientIdentityModelWebModule),
    typeof(AbpAutofacModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule),
    // Sufi Platform UI ABP Integration - bridges ABP services (menus, languages, users, etc.) to Sufi Platform UI
    typeof(AbpUiModule),
    // Sufi Platform UI Modules for Public Site
    // NOTE: SufiAccountBlazorModule is NOT included here because auth pages
    // are served by the dedicated AuthServer host in tiered architecture.
    typeof(SufiIdentityHttpApiClientModule),
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager">
    // File Manager for public file access (e.g., media in CMS pages)
    typeof(SufiFileManagerBlazorModule),
    typeof(SufiFileManagerBlazorServerModule),
    typeof(SufiFileManagerHttpApiClientModule),
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="module:tenants">
    // Tenant Management HTTP API Client (provides remote ITenantStore for tenant cookie resolution)
    typeof(SufiTenantsHttpApiClientModule),
    // </TEMPLATE-REMOVE>
    // SufiTheme using SufiBlazor design system
    // NOTE: For CMS, this will be replaced with dynamic layout rendering
    typeof(SufiThemeBlazorServerModule),
    // ABP Multi-Tenancy (cookie/header/domain resolvers + middleware)
    typeof(AbpAspNetCoreMultiTenancyModule)
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
        // Configure Sufi Platform bundling for additional app-specific styles
        Configure<BundleOptions>(options =>
        {
            options.StyleBundles.Add(BlazorSufiThemeBundles.Styles.Global, "/blazor-global-styles.css");
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
                options.FileSets.ReplaceEmbeddedByPhysical<SufiThemeBlazorServerModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}sufi-theme{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiTheme.Blazor.Server"));
                // Load SufiFramework and FileManager localization from source so base-type fallback and fa/ar work in dev
                options.FileSets.ReplaceEmbeddedByPhysical<UiDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, "..", "..", "..", "..", "..", "src", "framework", "src", "SufiChain.SufiPlatform.UI.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<FileManagerDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, "..", "..", "..", "..", "..", "src", "modules", "file-manager", "src", "SufiChain.SufiPlatform.FileManager.Domain.Shared"));
                // </TEMPLATE-REMOVE>
            });
        }
    }

    private void ConfigureLocalizationServices()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            // RTL is derived from culture by DefaultLanguageProvider (ar, fa, …)
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            options.Languages.Add(new LanguageInfo("fa", "fa", "فارسی"));
            options.Languages.Add(new LanguageInfo("ar", "ar", "العربية"));
            options.Languages.Add(new LanguageInfo("es", "es", "Español"));
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
        Configure<SufiRouterOptions>(options =>
        {
            options.AppAssembly = typeof(DemoAppBlazorWebSiteModule).Assembly;
        });
        
        // Configure SufiTheme for public site
        // TODO: In future CMS implementation, this will be replaced with dynamic layout selection
        // based on the CMS page being rendered. For now, use a simple sidebar layout.
        Configure<SufiThemeBlazorOptions>(options =>
        {
            options.Layout = SufiLayouts.TopMenu;
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
                .GetRequiredService<IOptions<SufiRouterOptions>>()
                .Value;
            
            endpoints.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddAdditionalAssemblies(routerOptions.AdditionalAssemblies.Distinct().ToArray());
        });
    }
}