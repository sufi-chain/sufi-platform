using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
// </TEMPLATE-REMOVE>
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
// <TEMPLATE-REMOVE IF-NOT="arch:single">
using Microsoft.OpenApi;
// </TEMPLATE-REMOVE>
using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.Account.Blazor;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI.Blazor;
using SufiChain.SufiAbp.AspNetCore.Authentication.OAuth;
using SufiChain.SufiAbp.AspNetCore.Authentication.OpenIdConnect;
using SufiChain.SufiAbp.AspNetCore.Authentication.Server;
using SufiChain.SufiAbp.AuditLogging;
using SufiChain.SufiAbp.AuditLogging.Blazor;
using SufiChain.SufiAbp.BackgroundJobs;
using SufiChain.SufiAbp.BackgroundJobs.Blazor;
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.FeatureManagement.Blazor;
using SufiChain.SufiAbp.Calendar;
using SufiChain.SufiAbp.Calendar.Blazor.Public;
using SufiChain.SufiAbp.FileManager;
using SufiChain.SufiAbp.FileManager.Blazor;
using SufiChain.SufiAbp.FileManager.Blazor.Server;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager-demo">
using SufiChain.SufiAbp.FileManager.Demo;
// </TEMPLATE-REMOVE>
using SufiChain.SufiAbp.FileManager.RichTextEditor;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.Blazor;
using SufiChain.SufiAbp.LocalizationManagement;
using SufiChain.SufiAbp.LocalizationManagement.Blazor;
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.SettingManagement.Blazor;
using SufiChain.SufiAbp.ShortLinkGenerator;
using SufiChain.SufiAbp.TagsManagement;
using SufiChain.SufiAbp.TenantManagement;
using SufiChain.SufiAbp.TenantManagement.Blazor;
using SufiChain.SufiAbp.UI;
using SufiChain.SufiAbp.MenuManagement;
using SufiChain.SufiAbp.MenuManagement.Blazor;
using SufiChain.SufiAbp.MenuManagement.Blazor.Server;




// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="module:localization-management">
// </TEMPLATE-REMOVE>
using SufiChain.SufiAbp.UI.Blazor.Server.MultiTenancy;
using SufiChain.SufiAbp.UI.Bundling;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using SufiChain.SufiAbp.UI.Toolbars;
using SufiChain.KomTheme;
using SufiChain.KomTheme.Blazor.Server;
// <TEMPLATE-REMOVE IF-NOT="module:tenant-management">
// </TEMPLATE-REMOVE>
using SufiChain.KomTheme.Blazor.Server.Bundling;
using SufiChain.SufiBlazor;
// <TEMPLATE-REMOVE IF-NOT="module:sufi-blazor-demo">
using SufiChain.SufiBlazor.Demo;
// </TEMPLATE-REMOVE>
using MyCompanyName.MyProjectName.Blazor.WebApp;

using MyCompanyName.MyProjectName.Data;
using MyCompanyName.MyProjectName.EntityFrameworkCore;
//using MyCompanyName.MyProjectName.MongoDB;
using MyCompanyName.MyProjectName.Localization;
using MyCompanyName.MyProjectName.Menus;
using MyCompanyName.MyProjectName.MultiTenancy;
using SufiChain.SufiAbp.AspNetCore.MultiTenancy;
using SufiChain.SufiAbp.AspNetCore.Serilog;
using SufiChain.SufiAbp.Autofac;
using SufiChain.SufiAbp.Caching;
// <TEMPLATE-REMOVE IF-NOT="arch:single">
using SufiChain.SufiAbp.Swashbuckle;
// </TEMPLATE-REMOVE>
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.Localization;
// <TEMPLATE-REMOVE IF-NOT="module:audit-logging">
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="module:background-jobs">
// </TEMPLATE-REMOVE>
using Volo.Abp.Data;
using SufiChain.SufiAbp.Messaging.Email;
// <TEMPLATE-REMOVE IF-NOT="db:mongodb">
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="db:mongodb">
// </TEMPLATE-REMOVE>
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
// <TEMPLATE-REMOVE IF-NOT="db:mongodb">
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="db:mongodb">
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="db:mongodb">
// </TEMPLATE-REMOVE>
using Volo.Abp.Threading;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.Uow;
using Volo.Abp.VirtualFileSystem;

namespace MyCompanyName.MyProjectName;

/// <summary>
/// Integrated WebApp module. Combines DB, Auth, API, and Blazor Server UI in one host.
/// No separate Application, Domain, HttpApi, or MongoDB projects — everything is in this project.
/// </summary>
[DependsOn(
    typeof(DemoAppApplicationModule),
    // <TEMPLATE-REMOVE IF-NOT="arch:single">
    typeof(DemoAppHttpApiModule),
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="db:efcore">
    typeof(DemoAppEntityFrameworkCoreModule),
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="db:mongodb">
    //typeof(DemoAppMongoDbModule),
    // </TEMPLATE-REMOVE>
    // =========================================================================
    // Core ABP Framework
    // =========================================================================
    typeof(SufiAbpAutofacModule),
    typeof(SufiAbpCachingModule),
    typeof(SufiAbpAspNetCoreMultiTenancyModule),
    typeof(SufiAbpAspNetCoreSerilogModule),
    // <TEMPLATE-REMOVE IF-NOT="arch:single">
    typeof(SufiAbpSwashbuckleModule),
    // </TEMPLATE-REMOVE>
    // =========================================================================
    // OpenIddict & Authentication
    // =========================================================================
	    typeof(SufiAbpAuthenticationOpenIdConnectModule),
	    typeof(SufiAbpAspNetCoreAuthenticationOAuthModule),
    typeof(SufiAbpAuthenticationServerModule),
    typeof(SufiAbpAccountBlazorModule),
    typeof(SufiAbpAccountApplicationModule),
    typeof(SufiAbpAccountHttpApiModule),
    typeof(SufiAbpPermissionManagementApplicationModule),
    typeof(SufiAbpPermissionManagementHttpApiModule),
    // Admin UI Modules
    // =========================================================================
    typeof(SufiAbpIdentityBlazorModule),
    typeof(SufiAbpIdentityApplicationModule),
    typeof(SufiAbpIdentityHttpApiModule),
    // <TEMPLATE-REMOVE IF-NOT="module:tenant-management">
    typeof(SufiAbpTenantManagementBlazorModule),
    typeof(SufiAbpTenantManagementApplicationModule),
    typeof(SufiAbpTenantManagementHttpApiModule),
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager">
    typeof(SufiAbpFileManagerBlazorModule),
    typeof(SufiAbpFileManagerBlazorServerModule),
    typeof(SufiAbpFileManagerRichTextEditorModule),
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager-demo">
    typeof(SufiAbpFileManagerDemoModule),
    // </TEMPLATE-REMOVE>
    typeof(SufiAbpFileManagerApplicationModule),
    typeof(SufiAbpFileManagerHttpApiModule),
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="module:audit-logging">
    // Audit Logging (UI + Application + HttpApi)
    typeof(SufiAbpAuditLoggingBlazorModule),
    typeof(SufiAbpAuditLoggingApplicationModule),
    typeof(SufiAbpAuditLoggingHttpApiModule),
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="module:background-jobs">
    // Background Jobs (UI + Application + HttpApi)
    typeof(SufiAbpBackgroundJobsBlazorModule),
    typeof(SufiAbpBackgroundJobsApplicationModule),
    typeof(SufiAbpBackgroundJobsHttpApiModule),
    // </TEMPLATE-REMOVE>
    typeof(SufiAbpFeatureManagementBlazorModule),
    typeof(SufiAbpFeatureManagementApplicationModule),
    typeof(SufiAbpFeatureManagementHttpApiModule),
    typeof(SufiAbpSettingManagementBlazorModule),
    typeof(SufiAbpSettingManagementApplicationModule),
    typeof(SufiAbpSettingManagementHttpApiModule),
    // <TEMPLATE-REMOVE IF-NOT="module:localization-management">
    // Localization Management (UI + Application + HttpApi)
    typeof(SufiAbpLocalizationManagementBlazorModule),
    typeof(SufiAbpLocalizationManagementApplicationModule),
    typeof(SufiAbpLocalizationManagementHttpApiModule),
    // </TEMPLATE-REMOVE>
    // =========================================================================
    // ABP MongoDB modules for infrastructure
    // =========================================================================
    // <TEMPLATE-REMOVE IF-NOT="db:mongodb">
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="db:mongodb">
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="db:mongodb">
    // </TEMPLATE-REMOVE>
    // =========================================================================
    // KomTheme using SufiBlazor design system
    // =========================================================================
    typeof(KomThemeBlazorServerModule),
    typeof(SufiAIBlazorModule),
    typeof(SufiAIApplicationModule),
    typeof(SufiAIHttpApiModule),
    // =========================================================================
    // Calendar
    // =========================================================================
    typeof(SufiAbpCalendarBlazorModule),
    typeof(SufiAbpCalendarBlazorPublicModule),
    typeof(SufiAbpCalendarAIModule),
    typeof(SufiAbpCalendarApplicationModule),
    typeof(SufiAbpCalendarHttpApiModule),
    // =========================================================================
    // Short Link Generator
    // =========================================================================
    typeof(SufiAbpShortLinkGeneratorBlazorModule),
    typeof(SufiAbpShortLinkGeneratorBlazorServerModule),
    typeof(SufiAbpShortLinkGeneratorApplicationModule),
    typeof(SufiAbpShortLinkGeneratorHttpApiModule),
    // =========================================================================
    // Tags Management (API only — no Blazor UI)
    // =========================================================================
    typeof(SufiAbpTagsManagementApplicationModule),
    typeof(SufiAbpTagsManagementHttpApiModule),
    // =========================================================================
    // Menu Management
    // =========================================================================
    typeof(SufiAbpMenuManagementBlazorModule),
    typeof(SufiAbpMenuManagementBlazorServerModule),
    typeof(SufiAbpMenuManagementApplicationModule),
    typeof(SufiAbpMenuManagementHttpApiModule),
    // <TEMPLATE-REMOVE IF-NOT="module:sufi-blazor-demo">
    // SufiBlazor component demo library
    typeof(SufiBlazorDemoModule)
    // </TEMPLATE-REMOVE>
)]
public class DemoAppModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(DemoAppResource),
                typeof(DemoAppApplicationContractsModule).Assembly
            );
        });

        // Configure OpenIddict server with local validation (self-hosted OIDC authority)
        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("DemoApp");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        // Add Blazor Web App services (Server-side interactive)
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

        // Add HttpClientFactory for Blazor components
        context.Services.AddHttpClient();

        if (hostingEnvironment.IsDevelopment())
        {
            context.Services.Replace(ServiceDescriptor.Singleton<IEmailSender, NullEmailSender>());
        }

        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureMultiTenancy();
        // <TEMPLATE-REMOVE IF-NOT="arch:single">
        ConfigureConventionalControllers();
        // </TEMPLATE-REMOVE>
        ConfigureIdentityLoginPath();
        ConfigureBundles();
        ConfigureLocalization();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureCors(context, configuration);
        ConfigureRouter(context);
        ConfigureMenu(configuration);
        // <TEMPLATE-REMOVE IF-NOT="arch:single">
        ConfigureSwaggerServices(context.Services);
        // </TEMPLATE-REMOVE>
        ConfigureAuditing();
        // Add SufiBlazor services 
        context.Services.AddSufiBlazor();

        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var authBuilder = context.Services.AddAuthentication();

        var googleClientId = configuration["ExternalAuth:Google:ClientId"];
        if (!string.IsNullOrEmpty(googleClientId))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = configuration["ExternalAuth:Google:ClientSecret"] ?? "";
                options.ClaimActions.MapAbpClaimTypes();
            });
        }

        var microsoftClientId = configuration["ExternalAuth:Microsoft:ClientId"];
        if (!string.IsNullOrEmpty(microsoftClientId))
        {
            authBuilder.AddMicrosoftAccount(options =>
            {
                options.ClientId = microsoftClientId;
                options.ClientSecret = configuration["ExternalAuth:Microsoft:ClientSecret"] ?? "";
                options.ClaimActions.MapAbpClaimTypes();
            });
        }

        var facebookAppId = configuration["ExternalAuth:Facebook:AppId"];
        if (!string.IsNullOrEmpty(facebookAppId))
        {
            authBuilder.AddFacebook(options =>
            {
                options.AppId = facebookAppId;
                options.AppSecret = configuration["ExternalAuth:Facebook:AppSecret"] ?? "";
                options.ClaimActions.MapAbpClaimTypes();
            });
        }
    }

    private void ConfigureAuditing()
    {
        // GET and HEAD are blacklisted by default (no audit logs for read operations).
        // To customize, e.g. add OPTIONS or use whitelist:
        // Configure<SpAuditingHttpMethodFilterOptions>(options =>
        // {
        //     options.BlacklistedHttpMethods.Add("OPTIONS");
        //     // Or whitelist only modifying methods: options.WhitelistedHttpMethods = new() { "POST", "PUT", "DELETE", "PATCH" };
        // });
    }

    private void ConfigureIdentityLoginPath()
    {
        // Configure ASP.NET Core Identity to redirect to Blazor login page
        Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
        {
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/account/access-denied";
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(
                configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());
        });
    }

    private void ConfigureMultiTenancy()
    {
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });
    }

    // <TEMPLATE-REMOVE IF-NOT="arch:single">
    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
        });
    }
    // </TEMPLATE-REMOVE>

    private void ConfigureBundles()
    {
        Configure<BundleOptions>(options =>
        {
            options.StyleBundles.Add(BlazorKomThemeBundles.Styles.Global, "/blazor-global-styles.css");
        });
    }

    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
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

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<DemoAppModule>();
            // Skip physical path replacement in Docker; use embedded resources (paths don't exist in container)
            if (hostingEnvironment.IsDevelopment() && Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
            {
                options.FileSets.ReplaceEmbeddedByPhysical<DemoAppApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}MyCompanyName.MyProjectName.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<DemoAppModule>(hostingEnvironment.ContentRootPath);
                // <TEMPLATE-REMOVE>
                options.FileSets.ReplaceEmbeddedByPhysical<KomThemeBlazorServerModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}independent-projects{Path.DirectorySeparatorChar}kom-theme{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.KomTheme.Blazor.Server"));
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAbpAccountBlazorModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}account{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiAbp.Account.Blazor"));
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAbpUiDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, "..", "..", "..", "..", "..", "src", "framework", "SufiChain.SufiAbp.UI.Domain.Shared"));
                // </TEMPLATE-REMOVE>
            }
        });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureRouter(ServiceConfigurationContext context)
    {
        Configure<SufiAbpRouterOptions>(options =>
        {
            // Add Account.Blazor assembly for Login, Register, etc. pages
            options.AdditionalAssemblies.Add(typeof(SufiAbpAccountBlazorModule).Assembly);
            // Add Client assembly for Index.razor (@page "/") — module not loaded, just route discovery
            options.AdditionalAssemblies.Add(typeof(DemoAppClientModule).Assembly);
        });

        Configure<KomThemeBlazorOptions>(options =>
        {
            options.Layout = KomLayouts.DualSidebar;
            options.IconRailDarkMode = true;
            options.ExpandOnHover = true;
            options.MobileShortcuts.Add(new MobileMenuShortcut(
                "DemoApp.Home",
                "Home",
                "/",
                "home"));
        });
    }

    private void ConfigureMenu(IConfiguration configuration)
    {
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new DemoAppMenuContributor(configuration));
        });
        Configure<ToolbarOptions>(options =>
        {
            options.Contributors.Add(new CalendarPublicToolbarContributor());
            options.Contributors.Add(new DemoAppToolbarContributor());
        });
    }

    // <TEMPLATE-REMOVE IF-NOT="arch:single">
    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "SufiChain SufiAbp Platform API", 
                    Version = "v1",
                    Description = "RESTful API for SufiChain SufiAbp Platform modules"
                });
                
                // Only include SufiChain.SufiAbp module APIs, exclude ABP framework endpoints
                options.DocInclusionPredicate((docName, description) =>
                {
                    if (description.ActionDescriptor is not Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor actionDescriptor)
                        return false;
                    
                    var controllerNamespace = actionDescriptor.ControllerTypeInfo.Namespace ?? "";
                    
                    // Exclude Volo.Abp.* controllers (ABP framework endpoints)
                    if (controllerNamespace.StartsWith("Volo.Abp"))
                        return false;
                    
                    // Include only SufiChain.SufiAbp.* controllers
                    return controllerNamespace.StartsWith("SufiChain.SufiAbp");
                });
                
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }
    // </TEMPLATE-REMOVE>
    
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
        app.UseCors();
        app.UseUnitOfWork();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseSpTenantSwitch();
            app.UseMultiTenancy();
        }

        app.UseAuthorization();

        // Required for Blazor Server antiforgery
        app.UseAntiforgery();

        // <TEMPLATE-REMOVE IF-NOT="arch:single">
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "DemoApp API");
        });
        // </TEMPLATE-REMOVE>

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();

        app.UseConfiguredEndpoints(endpoints =>
        {
            // Minimal health endpoint for Docker/load-balancer connectivity checks
            endpoints.MapGet("/health", () => Results.Ok(new { status = "ok" }));

            var routerOptions = endpoints.ServiceProvider
                .GetRequiredService<IOptions<SufiAbpRouterOptions>>()
                .Value;

            endpoints.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddAdditionalAssemblies(routerOptions.AdditionalAssemblies.Distinct().ToArray());
        });

    }
}
