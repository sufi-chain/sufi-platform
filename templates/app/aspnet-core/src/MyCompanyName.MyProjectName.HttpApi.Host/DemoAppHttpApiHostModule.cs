using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp.Autofac;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.Swashbuckle;
using Volo.Abp.AspNetCore.Serilog;
// <TEMPLATE-REMOVE IF-NOT="module:audit-logging">
using SufiChain.SufiPlatform.AuditLogging;
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="module:jobs">
// </TEMPLATE-REMOVE>
using SufiChain.SufiPlatform.Account;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager">
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;
using SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore;
// </TEMPLATE-REMOVE>
using SufiChain.SufiPlatform.Features;
// <TEMPLATE-REMOVE IF-NOT="db:efcore">
using SufiChain.SufiPlatform.Features.EntityFrameworkCore;
// </TEMPLATE-REMOVE>
using SufiChain.SufiPlatform.Identity;
// <TEMPLATE-REMOVE IF-NOT="module:localization">
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.Localization.EntityFrameworkCore;
// </TEMPLATE-REMOVE>
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Settings;
// <TEMPLATE-REMOVE IF-NOT="db:efcore">
using SufiChain.SufiPlatform.Settings.EntityFrameworkCore;
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="module:tenants">
using SufiChain.SufiPlatform.Tenants;
// </TEMPLATE-REMOVE>
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
// <TEMPLATE-REMOVE IF-NOT="db:efcore">
using MyCompanyName.MyProjectName.EntityFrameworkCore;
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="db:mongodb">
using MyCompanyName.MyProjectName.MongoDB;
// </TEMPLATE-REMOVE>
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
        typeof(AbpAutofacModule),
        typeof(AbpAspNetCoreMultiTenancyModule),
        typeof(DemoAppApplicationModule),
        typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
        typeof(AbpAspNetCoreSerilogModule),
        typeof(AbpSwashbuckleModule),
        // <TEMPLATE-REMOVE IF-NOT="db:efcore">
        typeof(DemoAppEntityFrameworkCoreModule),
        // </TEMPLATE-REMOVE>
        // <TEMPLATE-REMOVE IF-NOT="db:mongodb">
        typeof(DemoAppMongoDbModule),
        // </TEMPLATE-REMOVE>
        // <TEMPLATE-REMOVE IF-NOT="module:file-manager">
        // Blob storage for file manager (must load before SufiFileManagerEntityFrameworkCoreModule)
        typeof(SufiBlobDatabaseDatabaseEntityFrameworkCoreModule),
        // File Manager Module (data layer first for blob storage, then Application + API)
        typeof(SufiFileManagerEntityFrameworkCoreModule),
        typeof(SufiFileManagerApplicationModule),
        typeof(SufiFileManagerHttpApiModule),
        // </TEMPLATE-REMOVE>
        // <TEMPLATE-REMOVE IF-NOT="module:audit-logging">
        typeof(SufiAuditLoggingApplicationModule),
        typeof(SufiAuditLoggingHttpApiModule),
        // </TEMPLATE-REMOVE>
        // <TEMPLATE-REMOVE IF-NOT="module:jobs">
        typeof(SufiBackgroundJobsApplicationModule),
        typeof(SufiBackgroundJobsHttpApiModule),
        // </TEMPLATE-REMOVE>
        typeof(SufiIdentityApplicationModule),
        typeof(SufiIdentityHttpApiModule),
        // <TEMPLATE-REMOVE IF-NOT="module:tenants">
        // Tenant Management Module (Application services and API)
        typeof(SufiTenantsApplicationModule),
        typeof(SufiTenantsHttpApiModule),
        // </TEMPLATE-REMOVE>
        // Account Module (Application + HttpApi)
        typeof(SufiAccountApplicationModule),
        typeof(SufiAccountHttpApiModule),
        // Permission Management Module (Application + HttpApi)
        typeof(SufiPermissionsApplicationModule),
        typeof(SufiPermissionsHttpApiModule),
        // Feature Management Module (Application + HttpApi)
        typeof(SufiFeaturesApplicationModule),
        typeof(SufiFeaturesEntityFrameworkCoreModule),
        typeof(SufiFeaturesHttpApiModule),
        // Setting Management Module (Application + HttpApi)
        typeof(SufiSettingsApplicationModule),
        typeof(SufiSettingsEntityFrameworkCoreModule),
        typeof(SufiSettingsHttpApiModule),
        typeof(SufiAIApplicationModule),
        typeof(SufiAIHttpApiModule),
        // <TEMPLATE-REMOVE IF-NOT="module:localization">
        // Localization Management Module (backend services for translation editor)
        typeof(SufiLocalizationApplicationModule),
        typeof(SufiLocalizationHttpApiModule),
        typeof(SufiLocalizationEntityFrameworkCoreModule)
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
