using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Editors;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiBlazor;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Public;

[DependsOn(
    typeof(SufiFileManagerApplicationContractsModule)
)]
public class SufiFileManagerBlazorPublicModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<FileManagerOptions>(configuration.GetSection("FileManager"));
        PostConfigure<FileManagerOptions>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.FileAccessTokenSecret))
            {
                options.FileAccessTokenSecret = configuration["StringEncryption:DefaultPassPhrase"];
            }
        });

        // Register public file URL resolver
        context.Services.AddScoped<IFilePublicUrlResolver, FilePublicUrlResolver>();
        // Register file item URL provider (thumbnail/download/stream base URL; used by both public and admin)
        context.Services.AddScoped<IFileItemUrlProvider, FileItemUrlProvider>();
        context.Services.AddScoped<PublicFileUploadJsInterop>();
        context.Services.AddScoped<FileGalleryDialogService>();
        context.Services.AddScoped<IFileGalleryDialogService>(provider => provider.GetRequiredService<FileGalleryDialogService>());
        context.Services.AddMdToolbarContributor<FileManagerMarkdownToolbarContributor>();
    }
}