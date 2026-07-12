using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using SufiChain.SufiPlatform;
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.Account.Blazor.Server;
using SufiChain.SufiPlatform.SufiAI.Blazor;
using SufiChain.SufiPlatform.SufiAI.MongoDB;
using SufiChain.SufiPlatform.SufiAI.Pgvector;
using SufiChain.SufiPlatform.SufiAI.Qdrant;
using SufiChain.SufiPlatform.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AuditLogging.MongoDB;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Features.Blazor.Server;
using SufiChain.SufiPlatform.Features.MongoDB;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Blazor.Server;
using SufiChain.SufiPlatform.Identity.MongoDB;
using SufiChain.SufiPlatform.OpenIddict;
using SufiChain.SufiPlatform.OpenIddict.MongoDB;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Permissions.HttpApi;
using SufiChain.SufiPlatform.Permissions.Identity;
using SufiChain.SufiPlatform.Permissions.MongoDB;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Settings.Blazor.Server;
using SufiChain.SufiPlatform.Settings.MongoDB;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Tenants;
using SufiChain.SufiPlatform.Tenants.Blazor.Server;
using SufiChain.SufiPlatform.Tenants.MongoDB;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiTheme.Blazor;
using Volo.Abp.Threading;
using Volo.Abp.Data;
using SufiChain.SufiPlatform.Core;
using SufiChain.SufiPlatform.AspNetCore;
using Volo.Abp.VirtualFileSystem;
using Microsoft.OpenApi;

using Volo.Abp.Autofac;
using Volo.Abp.Localization;
using Volo.Abp.Swashbuckle;
using Volo.Abp.AspNetCore.Serilog;
namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    // AI Module
    typeof(SufiAIBlazorModule),
    typeof(SufiAIApplicationModule),
    typeof(SufiAIHttpApiModule),
    typeof(SufiAIMongoDbModule),
    typeof(SufiAIPgvectorModule),
    typeof(SufiAIQdrantModule),
    
    // Sufi Modules - MongoDB
    typeof(SufiAuditLoggingMongoDbModule),
    typeof(SufiIdentityMongoDbModule),
    typeof(SufiPermissionsMongoDbModule),
    typeof(SufiFeaturesMongoDbModule),
    typeof(SufiSettingsMongoDbModule),
    typeof(SufiTenantsMongoDbModule),
    typeof(SufiOpenIddictMongoDbModule),
    
    // Sufi Modules - Application & HttpApi
    typeof(SufiAccountApplicationModule),
    typeof(SufiAccountHttpApiModule),
    typeof(SufiIdentityApplicationModule),
    typeof(SufiIdentityHttpApiModule),
    typeof(SufiPermissionsApplicationModule),
    typeof(SufiPermissionsHttpApiModule),
    typeof(SufiPermissionsDomainIdentityModule),
    typeof(SufiFeaturesApplicationModule),
    typeof(SufiFeaturesHttpApiModule),
    typeof(SufiSettingsApplicationModule),
    typeof(SufiSettingsHttpApiModule),
    typeof(SufiTenantsApplicationModule),
    typeof(SufiTenantsHttpApiModule),
    
    // Sufi Modules - Blazor
    typeof(SufiAccountBlazorServerModule),
    typeof(SufiIdentityBlazorServerModule),
    typeof(SufiFeaturesBlazorServerModule),
    typeof(SufiSettingsBlazorServerModule),
    typeof(SufiTenantsBlazorServerModule),
    
    // SufiTheme
    typeof(SufiThemeBlazorModule),
    
    // Sufi Framework
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(SufiOpenIddictAspNetCoreModule)
)]
public class SufiAIBlazorHostModule : SufiModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureUrls(configuration);
        ConfigureAuthentication(context, configuration);
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureLocalizationServices();
        ConfigureSwaggerServices(context.Services);
        ConfigureAutoApiControllers();
        ConfigureCors(context, configuration);
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                options.Audience = "AI";
            });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<SufiVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAIDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiPlatform.SufiAI.Domain.Shared"));
                
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAIDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiPlatform.SufiAI.Domain"));
                
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAIApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiPlatform.SufiAI.Application.Contracts"));
                
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAIApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiPlatform.SufiAI.Application"));
                
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAIBlazorModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiPlatform.SufiAI.Blazor"));
            });
        }
    }

    private void ConfigureLocalizationServices()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            options.Languages.Add(new LanguageInfo("ar", "ar", "العربية"));
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "AI API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<SufiAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(SufiAIApplicationModule).Assembly);
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
                    .WithSufiExposedHeaders()
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
            app.UseErrorPage();
        }

        app.UseCorrelationId();
        app.MapAbpStaticAssets();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI API");
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();

        using (var scope = context.ServiceProvider.CreateScope())
        {
            AsyncHelper.RunSync(async () =>
            {
                await scope.ServiceProvider
                    .GetRequiredService<IDataSeeder>()
                    .SeedAsync();
            });
        }
    }
}
