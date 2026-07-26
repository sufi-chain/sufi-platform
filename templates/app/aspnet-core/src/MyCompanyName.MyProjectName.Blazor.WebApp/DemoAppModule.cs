using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Volo.Abp.Autofac;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Authentication.OAuth;
using Volo.Abp.Swashbuckle;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Caching;
// </TEMPLATE-REMOVE>
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
// <TEMPLATE-REMOVE IF-NOT="arch:single">
using Microsoft.OpenApi;
// </TEMPLATE-REMOVE>
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.Account.Blazor;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Blazor;
using SufiChain.SufiPlatform.AspNetCore.Authentication.OpenIdConnect;
using SufiChain.SufiPlatform.AspNetCore.Authentication.Server;
using SufiChain.SufiPlatform.AuditLogging;
using SufiChain.SufiPlatform.AuditLogging.Blazor;
using SufiChain.SufiPlatform.BackgroundJobs.Blazor;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Features.Blazor;
using SufiChain.SufiPlatform.Calendar;
using SufiChain.SufiPlatform.Calendar.Blazor.Public;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.FileManager.Blazor;
using SufiChain.SufiPlatform.FileManager.Blazor.Public;
using SufiChain.SufiPlatform.FileManager.Blazor.Server;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager-demo">
using SufiChain.SufiPlatform.FileManager.Demo;
// </TEMPLATE-REMOVE>
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Blazor;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.Localization.Blazor;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Settings.Blazor;
using SufiChain.SufiPlatform.ShortLinks;
using SufiChain.SufiPlatform.Tags;
using SufiChain.SufiPlatform.Tenants;
using SufiChain.SufiPlatform.Tenants.Blazor;
using SufiChain.SufiPlatform.Menus;
using SufiChain.SufiPlatform.Menus.Blazor;
using SufiChain.SufiPlatform.Menus.Blazor.Server;




// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="module:localization">
// </TEMPLATE-REMOVE>
using SufiChain.SufiPlatform.UI.Blazor.Server.MultiTenancy;
using SufiChain.SufiPlatform.UI.Bundling;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using SufiChain.SufiPlatform.UI.Toolbars;
using SufiChain.SufiTheme;
using SufiChain.SufiTheme.Blazor.Server;
// <TEMPLATE-REMOVE IF-NOT="module:tenants">
// </TEMPLATE-REMOVE>
using SufiChain.SufiTheme.Blazor.Server.Bundling;
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
// <TEMPLATE-REMOVE IF-NOT="arch:single">
// </TEMPLATE-REMOVE>
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.Localization;
// <TEMPLATE-REMOVE IF-NOT="module:audit-logging">
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="module:jobs">
// </TEMPLATE-REMOVE>
using Volo.Abp.Data;
using SufiChain.SufiPlatform.SufiCom.Email;
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

using SufiChain.SufiPlatform.UI;

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
    typeof(AbpAutofacModule),
    typeof(AbpCachingModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreSerilogModule),
    // <TEMPLATE-REMOVE IF-NOT="arch:single">
    typeof(AbpSwashbuckleModule),
    // </TEMPLATE-REMOVE>
    // =========================================================================
    // OpenIddict & Authentication
    // =========================================================================
	    typeof(SufiAuthenticationOpenIdConnectModule),
	    typeof(AbpAspNetCoreAuthenticationOAuthModule),
    typeof(SufiAuthenticationServerModule),
    typeof(SufiAccountBlazorModule),
    typeof(SufiAccountApplicationModule),
    typeof(SufiAccountHttpApiModule),
    typeof(SufiPermissionsApplicationModule),
    typeof(SufiPermissionsHttpApiModule),
    // Admin UI Modules
    // =========================================================================
    typeof(SufiIdentityBlazorModule),
    typeof(SufiIdentityApplicationModule),
    typeof(SufiIdentityHttpApiModule),
    // <TEMPLATE-REMOVE IF-NOT="module:tenants">
    typeof(SufiTenantsBlazorModule),
    typeof(SufiTenantsApplicationModule),
    typeof(SufiTenantsHttpApiModule),
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager">
    typeof(SufiFileManagerBlazorModule),
    typeof(SufiFileManagerBlazorPublicModule),
    typeof(SufiFileManagerBlazorServerModule),
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager-demo">
    typeof(SufiFileManagerDemoModule),
    // </TEMPLATE-REMOVE>
    typeof(SufiFileManagerApplicationModule),
    typeof(SufiFileManagerHttpApiModule),
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="module:audit-logging">
    // Audit Logging (UI + Application + HttpApi)
    typeof(SufiAuditLoggingBlazorModule),
    typeof(SufiAuditLoggingApplicationModule),
    typeof(SufiAuditLoggingHttpApiModule),
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="module:jobs">
    // Background Jobs (UI + Application + HttpApi)
    typeof(SufiBackgroundJobsBlazorModule),
    typeof(SufiBackgroundJobsApplicationModule),
    typeof(SufiBackgroundJobsHttpApiModule),
    // </TEMPLATE-REMOVE>
    typeof(SufiFeaturesBlazorModule),
    typeof(SufiFeaturesApplicationModule),
    typeof(SufiFeaturesHttpApiModule),
    typeof(SufiSettingsBlazorModule),
    typeof(SufiSettingsApplicationModule),
    typeof(SufiSettingsHttpApiModule),
    // <TEMPLATE-REMOVE IF-NOT="module:localization">
    // Localization Management (UI + Application + HttpApi)
    typeof(SufiLocalizationBlazorModule),
    typeof(SufiLocalizationApplicationModule),
    typeof(SufiLocalizationHttpApiModule),
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
    // SufiTheme using SufiBlazor design system
    // =========================================================================
    typeof(SufiThemeBlazorServerModule),
    typeof(SufiAIBlazorModule),
    typeof(SufiAIApplicationModule),
    typeof(SufiAIHttpApiModule),
    // =========================================================================
    // Calendar
    // =========================================================================
    typeof(SufiCalendarBlazorModule),
    typeof(SufiCalendarBlazorPublicModule),
    typeof(SufiCalendarAIModule),
    typeof(SufiCalendarApplicationModule),
    typeof(SufiCalendarHttpApiModule),
    // =========================================================================
    // Short Link Generator
    // =========================================================================
    typeof(SufiShortLinksBlazorModule),
    typeof(SufiShortLinksBlazorServerModule),
    typeof(SufiShortLinksApplicationModule),
    typeof(SufiShortLinksHttpApiModule),
    // =========================================================================
    // Tags Management (API only — no Blazor UI)
    // =========================================================================
    typeof(SufiTagsApplicationModule),
    typeof(SufiTagsHttpApiModule),
    // =========================================================================
    // Menu Management
    // =========================================================================
    typeof(SufiMenusBlazorModule),
    typeof(SufiMenusBlazorServerModule),
    typeof(SufiMenusApplicationModule),
    typeof(SufiMenusHttpApiModule),
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
            options.StyleBundles.Add(BlazorSufiThemeBundles.Styles.Global, "/blazor-global-styles.css");
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
                options.FileSets.ReplaceEmbeddedByPhysical<SufiThemeBlazorServerModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}independent-projects{Path.DirectorySeparatorChar}sufi-theme{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiTheme.Blazor.Server"));
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAccountBlazorModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}account{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiPlatform.Account.Blazor"));
                options.FileSets.ReplaceEmbeddedByPhysical<SufiUiDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, "..", "..", "..", "..", "..", "src", "framework", "SufiChain.SufiPlatform.UI.Domain.Shared"));
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
        Configure<SufiRouterOptions>(options =>
        {
            // Add Account.Blazor assembly for Login, Register, etc. pages
            options.AdditionalAssemblies.Add(typeof(SufiAccountBlazorModule).Assembly);
            // Add Client assembly for Index.razor (@page "/") — module not loaded, just route discovery
            options.AdditionalAssemblies.Add(typeof(DemoAppClientModule).Assembly);
        });

        Configure<SufiThemeBlazorOptions>(options =>
        {
            options.Layout = SufiLayouts.DualSidebar;
            options.IconRailDarkMode = true;
            options.ExpandOnHover = true;
            options.IconRailHomeUrl = "/panel/dashboard";
            options.MobileShortcuts.Add(new MobileMenuShortcut(
                "DemoApp.Home",
                "Home",
                "/",
                "home"));
        });
    }

    private void ConfigureMenu(IConfiguration configuration)
    {
        Configure<SufiNavigationOptions>(options =>
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
                    Title = "SufiChain Sufi Platform Platform API", 
                    Version = "v1",
                    Description = "RESTful API for SufiChain Sufi Platform Platform modules"
                });
                
                // Only include SufiChain.SufiPlatform module APIs, exclude ABP framework endpoints
                options.DocInclusionPredicate((docName, description) =>
                {
                    if (description.ActionDescriptor is not Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor actionDescriptor)
                        return false;
                    
                    var controllerNamespace = actionDescriptor.ControllerTypeInfo.Namespace ?? "";
                    
                    // Exclude Volo.Abp.* controllers (ABP framework endpoints)
                    if (controllerNamespace.StartsWith("Volo.Abp"))
                        return false;
                    
                    // Include only SufiChain.SufiPlatform.* controllers
                    return controllerNamespace.StartsWith("SufiChain.SufiPlatform");
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
                .GetRequiredService<IOptions<SufiRouterOptions>>()
                .Value;

            endpoints.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddAdditionalAssemblies(routerOptions.AdditionalAssemblies.Distinct().ToArray());
        });

    }
}