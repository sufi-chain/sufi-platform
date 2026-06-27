using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using SufiChain.SufiAbp;
using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.Account.Blazor.Server;
using SufiChain.SufiAbp.AI.Blazor;
using SufiChain.SufiAbp.AI.MongoDB;
using SufiChain.SufiAbp.AI.Pgvector;
using SufiChain.SufiAbp.AI.Qdrant;
using SufiChain.SufiAbp.AspNetCore.Mvc;
using SufiChain.SufiAbp.AspNetCore.Serilog;
using SufiChain.SufiAbp.AuditLogging.MongoDB;
using SufiChain.SufiAbp.Autofac;
using SufiChain.SufiAbp.Data;
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.FeatureManagement.Blazor.Server;
using SufiChain.SufiAbp.FeatureManagement.MongoDB;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.Blazor.Server;
using SufiChain.SufiAbp.Identity.MongoDB;
using SufiChain.SufiAbp.Localization;
using SufiChain.SufiAbp.Modularity;
using SufiChain.SufiAbp.MultiTenancy;
using SufiChain.SufiAbp.OpenIddict;
using SufiChain.SufiAbp.OpenIddict.MongoDB;
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.PermissionManagement.HttpApi;
using SufiChain.SufiAbp.PermissionManagement.Identity;
using SufiChain.SufiAbp.PermissionManagement.MongoDB;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.SettingManagement.Blazor.Server;
using SufiChain.SufiAbp.SettingManagement.MongoDB;
using SufiChain.SufiAbp.Swashbuckle;
using SufiChain.SufiAbp.TenantManagement;
using SufiChain.SufiAbp.TenantManagement.Blazor.Server;
using SufiChain.SufiAbp.TenantManagement.MongoDB;
using SufiChain.SufiAbp.Threading;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.VirtualFileSystem;
using SufiChain.KomTheme.Blazor;
using Volo.Abp.Threading;
using Volo.Abp.Data;
using SufiChain.SufiAbp.Core;
using SufiChain.SufiAbp.AspNetCore;
using Volo.Abp.VirtualFileSystem;
using Microsoft.OpenApi;

namespace SufiChain.SufiAbp.AI;

[DependsOn(
    // AI Module
    typeof(SufiAIBlazorModule),
    typeof(SufiAIApplicationModule),
    typeof(SufiAIHttpApiModule),
    typeof(SufiAIMongoDbModule),
    typeof(SufiAIPgvectorModule),
    typeof(SufiAIQdrantModule),
    
    // SufiAbp Modules - MongoDB
    typeof(SufiAbpAuditLoggingMongoDbModule),
    typeof(SufiAbpIdentityMongoDbModule),
    typeof(SufiAbpPermissionManagementMongoDbModule),
    typeof(SufiAbpFeatureManagementMongoDbModule),
    typeof(SufiAbpSettingManagementMongoDbModule),
    typeof(SufiAbpTenantManagementMongoDbModule),
    typeof(SufiAbpOpenIddictMongoDbModule),
    
    // SufiAbp Modules - Application & HttpApi
    typeof(SufiAbpAccountApplicationModule),
    typeof(SufiAbpAccountHttpApiModule),
    typeof(SufiAbpIdentityApplicationModule),
    typeof(SufiAbpIdentityHttpApiModule),
    typeof(SufiAbpPermissionManagementApplicationModule),
    typeof(SufiAbpPermissionManagementHttpApiModule),
    typeof(SufiAbpPermissionManagementDomainIdentityModule),
    typeof(SufiAbpFeatureManagementApplicationModule),
    typeof(SufiAbpFeatureManagementHttpApiModule),
    typeof(SufiAbpSettingManagementApplicationModule),
    typeof(SufiAbpSettingManagementHttpApiModule),
    typeof(SufiAbpTenantManagementApplicationModule),
    typeof(SufiAbpTenantManagementHttpApiModule),
    
    // SufiAbp Modules - Blazor
    typeof(SufiAbpAccountBlazorServerModule),
    typeof(SufiAbpIdentityBlazorServerModule),
    typeof(SufiAbpFeatureManagementBlazorServerModule),
    typeof(SufiAbpSettingManagementBlazorServerModule),
    typeof(SufiAbpTenantManagementBlazorServerModule),
    
    // KomTheme
    typeof(SufiAbpKomThemeBlazorModule),
    
    // SufiAbp Framework
    typeof(SufiAbpAutofacModule),
    typeof(SufiAbpAspNetCoreSerilogModule),
    typeof(SufiAbpSwashbuckleModule),
    typeof(SufiAbpOpenIddictAspNetCoreModule)
)]
public class AIBlazorHostModule : SufiAbpModule
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
            Configure<SufiAbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAIDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiAbp.AI.Domain.Shared"));
                
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAIDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiAbp.AI.Domain"));
                
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAIApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiAbp.AI.Application.Contracts"));
                
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAIApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiAbp.AI.Application"));
                
                options.FileSets.ReplaceEmbeddedByPhysical<SufiAIBlazorModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, 
                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}SufiChain.SufiAbp.AI.Blazor"));
            });
        }
    }

    private void ConfigureLocalizationServices()
    {
        Configure<SufiAbpLocalizationOptions>(options =>
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
        Configure<SufiAbpAspNetCoreMvcOptions>(options =>
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
                    .WithSufiAbpExposedHeaders()
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
