using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Validation.AspNetCore;
using SufiChain.SufiAbp.AspNetCore;
using SufiChain.SufiAbp.AspNetCore.Authentication.OpenIdConnect;
using SufiChain.SufiAbp.AspNetCore.Authentication.Server;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Account.Blazor;
using Volo.Abp.Autofac;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Authentication.OAuth;
using Volo.Abp.AspNetCore.Serilog;
// <TEMPLATE-REMOVE IF-NOT="module:tenant-management">
using SufiChain.SufiAbp.TenantManagement;
// </TEMPLATE-REMOVE>
using SufiChain.SufiTheme;
using SufiChain.SufiTheme.Blazor.Server;
using SufiChain.SufiAbp.UI.Blazor.Server.MultiTenancy;
using SufiChain.SufiAbp.UI.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volo.Abp;
using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.PermissionManagement;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Auditing;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.FeatureManagement.MongoDB;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.SettingManagement.MongoDB;
using Volo.Abp.Threading;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using MyCompanyName.MyProjectName.MongoDB;
using MyCompanyName.MyProjectName.MultiTenancy;

namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(AbpAspNetCoreMultiTenancyModule),
        typeof(SufiAbpAspNetCoreModule),
        typeof(DemoAppApplicationModule),
        typeof(DemoAppMongoDbModule),
        // OpenIddict authorization endpoints (authorize, token, logout)
        typeof(SufiAbpAuthenticationOpenIdConnectModule),
        // OAuth external logins (Google, Microsoft, Facebook) - provides MapAbpClaimTypes
        typeof(AbpAspNetCoreAuthenticationOAuthModule),
        // SufiAbp Account controller (complete-login, OIDC Login/Logout, ExternalLogin)
        typeof(SufiAbpAuthenticationServerModule),
        // Blazor Server theme for rendering auth UI pages
        typeof(SufiThemeBlazorServerModule),
        // Account Blazor pages (Login, Register, etc.)
        typeof(SufiAbpAccountBlazorModule),
        typeof(AbpAspNetCoreSerilogModule),
        // Identity services for credential validation (SignInManager, UserManager)
        typeof(SufiAbpIdentityApplicationModule),
        typeof(SufiAbpIdentityHttpApiModule),
        // Tenant Management for multi-tenancy resolution during login
        typeof(SufiAbpTenantManagementApplicationModule),
        typeof(SufiAbpTenantManagementHttpApiModule),
        typeof(SufiAbpAccountApplicationModule),
        typeof(SufiAbpAccountHttpApiModule),
        typeof(SufiAbpPermissionManagementApplicationModule),
        typeof(SufiAbpPermissionManagementHttpApiModule),
        typeof(SufiAbpFeatureManagementApplicationModule),
        typeof(SufiAbpFeatureManagementHttpApiModule),
        typeof(SufiAbpFeatureManagementMongoDbModule),
        typeof(SufiAbpSettingManagementApplicationModule),
        typeof(SufiAbpSettingManagementHttpApiModule),
        typeof(SufiAbpSettingManagementMongoDbModule)
    )]
    public class DemoAppAuthServerModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            // Configure OpenIddict server with local validation
            // This is the OIDC authority -- tokens are issued and validated here
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
            var configuration = context.Services.GetConfiguration();
            var hostingEnvironment = context.Services.GetHostingEnvironment();

            // Add Blazor with Interactive Server for authentication UI (login forms, tenant selector)
            // Login form uses native POST so sign-in runs in HTTP request context and cookie is set.
            context.Services.AddRazorComponents()
                .AddInteractiveServerComponents(options =>
                {
                    if (hostingEnvironment.IsDevelopment())
                    {
                        options.DetailedErrors = true;
                    }
                });

            // Add HttpClientFactory for Blazor components to make HTTP requests
            context.Services.AddHttpClient();

            ConfigureAuthentication(context);
            ConfigureUrls(configuration);
            ConfigureConventionalControllers();
            ConfigureIdentityLoginPath();
            ConfigureLocalization();
            ConfigureVirtualFileSystem(context);
            ConfigureCors(context, configuration);
            ConfigureRouter(context);
            ConfigureAuditing();

            Configure<AbpMvcLibsOptions>(options =>
            {
                options.CheckLibs = false;
            });
        }

        private void ConfigureAuthentication(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();

            //context.Services.AddAuthentication()
            //    .AddGoogle(options =>
            //    {
            //        options.ClientId = configuration["ExternalAuth:Google:ClientId"] ?? "";
            //        options.ClientSecret = configuration["ExternalAuth:Google:ClientSecret"] ?? "";
            //        options.ClaimActions.MapAbpClaimTypes();
            //    })
            //    .AddMicrosoftAccount(options =>
            //    {
            //        options.ClientId = configuration["ExternalAuth:Microsoft:ClientId"] ?? "";
            //        options.ClientSecret = configuration["ExternalAuth:Microsoft:ClientSecret"] ?? "";
            //        options.ClaimActions.MapAbpClaimTypes();
            //    })
            //    .AddFacebook(options =>
            //    {
            //        options.AppId = configuration["ExternalAuth:Facebook:AppId"] ?? "";
            //        options.AppSecret = configuration["ExternalAuth:Facebook:AppSecret"] ?? "";
            //        options.ClaimActions.MapAbpClaimTypes();
            //    });

            context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
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
                options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());
            });
        }

        private void ConfigureConventionalControllers()
        {
            // SufiAbp HttpApi modules handle Account, Permission, Feature, Setting routes via explicit controllers.
            // No conventional controller setup needed for these modules.
        }

        private void ConfigureLocalization()
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
        }

        private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
        {
            var hostingEnvironment = context.Services.GetHostingEnvironment();

            if (hostingEnvironment.IsDevelopment())
            {
                Configure<AbpVirtualFileSystemOptions>(options =>
                {
                    options.FileSets.ReplaceEmbeddedByPhysical<DemoAppDomainSharedModule>(
                        Path.Combine(hostingEnvironment.ContentRootPath,
                            $"..{Path.DirectorySeparatorChar}MyCompanyName.MyProjectName.Domain.Shared"));
                    options.FileSets.ReplaceEmbeddedByPhysical<DemoAppDomainModule>(
                        Path.Combine(hostingEnvironment.ContentRootPath,
                            $"..{Path.DirectorySeparatorChar}MyCompanyName.MyProjectName.Domain"));
                    options.FileSets.ReplaceEmbeddedByPhysical<DemoAppApplicationContractsModule>(
                        Path.Combine(hostingEnvironment.ContentRootPath,
                            $"..{Path.DirectorySeparatorChar}MyCompanyName.MyProjectName.Application.Contracts"));
                    options.FileSets.ReplaceEmbeddedByPhysical<DemoAppApplicationModule>(
                        Path.Combine(hostingEnvironment.ContentRootPath,
                            $"..{Path.DirectorySeparatorChar}MyCompanyName.MyProjectName.Application"));
                    // <TEMPLATE-REMOVE>
                    // Theme modules for hot reload during development
                    options.FileSets.ReplaceEmbeddedByPhysical<SufiThemeBlazorServerModule>(
                        Path.Combine(hostingEnvironment.ContentRootPath,
                            $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}sufi-theme{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiTheme.Blazor.Server"));
                    options.FileSets.ReplaceEmbeddedByPhysical<SufiAbpAccountBlazorModule>(
                        Path.Combine(hostingEnvironment.ContentRootPath,
                            $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}modules{Path.DirectorySeparatorChar}account{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiAbp.Account.Blazor"));
                    // </TEMPLATE-REMOVE>
                });
            }
        }

        private void ConfigureRouter(ServiceConfigurationContext context)
        {
            Configure<SufiAbpRouterOptions>(options =>
            {
                options.AdditionalAssemblies.Add(typeof(DemoAppAuthServerModule).Assembly);
                // Add Account.Blazor assembly for Login, Register, etc. pages
                options.AdditionalAssemblies.Add(typeof(SufiAbpAccountBlazorModule).Assembly);
            });

            // Configure SufiTheme to use DualSidebar layout with modern dual sidebar pattern
            Configure<SufiThemeBlazorOptions>(options =>
            {
                options.Layout = SufiLayouts.DualSidebar;
                options.IconRailDarkMode = true;
                options.ExpandOnHover = true;
            });
        }

        private void ConfigureAuditing()
        {
            // NOTE: Entity History (EntityChanges) is an EF Core-only feature in ABP.
            // MongoDB does not support property-level change tracking.
            // Actions and general audit info (HTTP method, URL, user, etc.) still work.
        }

        private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .WithOrigins(
                            configuration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .WithAbpExposedHeaders()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

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
            app.UseAuthentication();
            app.UseAbpOpenIddictValidation();

            if (MultiTenancyConsts.IsEnabled)
            {
                app.UseSpTenantSwitch();
                app.UseMultiTenancy();
            }

            app.UseUnitOfWork();
            app.UseAuthorization();

            // Required for Blazor Server antiforgery
            app.UseAntiforgery();

            app.UseAuditing();
            app.UseAbpSerilogEnrichers();

            app.UseConfiguredEndpoints(endpoints =>
            {
                // Map Blazor Static SSR components for authentication pages
                endpoints.MapRazorComponents<App>()
                    .AddInteractiveServerRenderMode()
                    .AddAdditionalAssemblies(typeof(SufiAbpAccountBlazorModule).Assembly);
            });
        }
    }

}
