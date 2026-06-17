using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SufiChain.SufiAbp.AIManagement;
// <TEMPLATE-REMOVE IF-NOT="module:audit-logging">
using SufiChain.SufiAbp.AuditLogging;
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="module:background-jobs">
using SufiChain.SufiAbp.BackgroundJobs;
// </TEMPLATE-REMOVE>
using SufiChain.SufiAbp.Account;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager">
using SufiChain.SufiAbp.FileManager;
using SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;
// </TEMPLATE-REMOVE>
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.Identity;
// <TEMPLATE-REMOVE IF-NOT="module:localization-management">
using SufiChain.SufiAbp.LocalizationManagement;
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="module:sufi-blazor-demo">
using SufiChain.SufiBlazor.Demo;
// </TEMPLATE-REMOVE>
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.SettingManagement;
// <TEMPLATE-REMOVE IF-NOT="module:tenant-management">
using SufiChain.SufiAbp.TenantManagement;
// </TEMPLATE-REMOVE>
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using SufiChain.SufiAbp.AspNetCore.Authentication.JwtBearer;
using SufiChain.SufiAbp.AspNetCore.MultiTenancy;
using SufiChain.SufiAbp.AspNetCore.Serilog;
using SufiChain.SufiAbp.Autofac;
using SufiChain.SufiAbp.Swashbuckle;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using MyCompanyName.MyProjectName.MultiTenancy;

namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(DemoAppHttpApiModule),
        typeof(SufiAbpAutofacModule),
        typeof(SufiAbpAspNetCoreMultiTenancyModule),
        typeof(DemoAppApplicationModule),
        typeof(SufiAbpAspNetCoreAuthenticationJwtBearerModule),
        typeof(SufiAbpAspNetCoreSerilogModule),
        typeof(SufiAbpSwashbuckleModule),
        // <TEMPLATE-REMOVE IF-NOT="module:file-manager">
        // Blob storage for file manager (must load before SufiAbpFileManagerEntityFrameworkCoreModule)
        typeof(SufiAbpBlobStoringDatabaseEntityFrameworkCoreModule),
        // File Manager Module (data layer first for blob storage, then Application + API)
        typeof(SufiAbpFileManagerEntityFrameworkCoreModule),
        typeof(SufiAbpFileManagerApplicationModule),
        typeof(SufiAbpFileManagerHttpApiModule),
        // </TEMPLATE-REMOVE>
        // <TEMPLATE-REMOVE IF-NOT="module:audit-logging">
        typeof(SufiAbpAuditLoggingApplicationModule),
        typeof(SufiAbpAuditLoggingHttpApiModule),
        // </TEMPLATE-REMOVE>
        // <TEMPLATE-REMOVE IF-NOT="module:background-jobs">
        typeof(SufiAbpBackgroundJobsApplicationModule),
        typeof(SufiAbpBackgroundJobsHttpApiModule),
        // </TEMPLATE-REMOVE>
        typeof(SufiAbpIdentityApplicationModule),
        typeof(SufiAbpIdentityHttpApiModule),
        // <TEMPLATE-REMOVE IF-NOT="module:tenant-management">
        // Tenant Management Module (Application services and API)
        typeof(SufiAbpTenantManagementApplicationModule),
        typeof(SufiAbpTenantManagementHttpApiModule),
        // </TEMPLATE-REMOVE>
        // Account Module (Application + HttpApi)
        typeof(SufiAbpAccountApplicationModule),
        typeof(SufiAbpAccountHttpApiModule),
        // Permission Management Module (Application + HttpApi)
        typeof(SufiAbpPermissionManagementApplicationModule),
        typeof(SufiAbpPermissionManagementHttpApiModule),
        // Feature Management Module (Application + HttpApi)
        typeof(SufiAbpFeatureManagementApplicationModule),
        typeof(SufiAbpFeatureManagementEntityFrameworkCoreModule),
        typeof(SufiAbpFeatureManagementHttpApiModule),
        // Setting Management Module (Application + HttpApi)
        typeof(SufiAbpSettingManagementApplicationModule),
        typeof(SufiAbpSettingManagementEntityFrameworkCoreModule),
        typeof(SufiAbpSettingManagementHttpApiModule),
        typeof(SufiAbpAIManagementApplicationModule),
        typeof(SufiAbpAIManagementHttpApiModule),
        // <TEMPLATE-REMOVE IF-NOT="module:localization-management">
        // Localization Management Module (backend services for translation editor)
        typeof(SufiAbpLocalizationManagementApplicationModule),
        typeof(SufiAbpLocalizationManagementHttpApiModule),
        typeof(SufiAbpLocalizationManagementEntityFrameworkCoreModule),
        // </TEMPLATE-REMOVE>
        // <TEMPLATE-REMOVE IF-NOT="module:sufi-blazor-demo">
        // SufiBlazor Demo localization (Blazor fetches from remote API)
        typeof(SufiBlazorDemoLocalizationModule)
        // </TEMPLATE-REMOVE>
    )]
    public class DemoAppHttpApiHostModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostingEnvironment = context.Services.GetHostingEnvironment();

            ConfigureConventionalControllers();
            ConfigureAuthentication(context, configuration);
            ConfigureLocalization();
            ConfigureVirtualFileSystem(context);
            ConfigureCors(context, configuration);
            ConfigureSwaggerServices(context, configuration);
            ConfigureAuditing();

            Configure<AbpMvcLibsOptions>(options =>
            {
                options.CheckLibs = false;
            });
        }

        private void ConfigureAuditing()
        {
            // Entity History (EntityChanges) is now enabled with EF Core.
            // Property-level change tracking is supported via EF Core's change tracker.
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
                });
            }
        }

        private void ConfigureConventionalControllers()
        {
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(DemoAppApplicationModule).Assembly);
            });
        }

        private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["AuthServer:Authority"];
                    options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                    options.Audience = "DemoApp";
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                });
        }

        private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddAbpSwaggerGenWithOAuth(
                configuration["AuthServer:Authority"],
                new Dictionary<string, string>
                {
                    {"DemoApp", "DemoApp API"}
                },
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo {Title = "DemoApp API", Version = "v1"});
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);
                });
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

        private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddCors(options =>
            {
                options.AddDefaultPolicy( builder =>
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
            app.UseJwtTokenMiddleware();

            if (MultiTenancyConsts.IsEnabled)
            {
                app.UseMultiTenancy();
            }

            app.UseUnitOfWork();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseAbpSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DemoApp API");

                var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
                c.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
                c.OAuthClientSecret(configuration["AuthServer:SwaggerClientSecret"]);
                c.OAuthScopes("DemoApp");
            });

            app.UseAuditing();
            app.UseAbpSerilogEnrichers();

            app.UseConfiguredEndpoints();
        }
    }
}
