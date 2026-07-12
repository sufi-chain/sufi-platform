using SufiChain.SufiPlatform.UI.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.AspNetCore.Mvc;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.BlobStoring;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager;

[DependsOn(
    typeof(SufiFileManagerApplicationContractsModule),
    typeof(SufiAspNetCoreMvcModule),
    typeof(AbpBlobStoringModule))]
public class SufiFileManagerHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<FileManagerOptions>(configuration.GetSection("FileManager"));
        PostConfigure<FileManagerOptions>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                options.BaseUrl = configuration["App:SelfUrl"];
            }
            if (!string.IsNullOrEmpty(options.BaseUrl) && !options.BaseUrl.EndsWith("/"))
            {
                options.BaseUrl += "/";
            }
            if (string.IsNullOrWhiteSpace(options.FileAccessTokenSecret))
            {
                options.FileAccessTokenSecret = configuration["StringEncryption:DefaultPassPhrase"];
            }
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<SufiFileManagerResource>()
                .AddBaseTypes(typeof(SufiUiResource));
        });
    }
}