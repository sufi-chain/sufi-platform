using SufiChain.SufiAbp.UI.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AspNetCore.Mvc;
using SufiChain.SufiAbp.BlobStoring;
using SufiChain.SufiAbp.FileManager.Configuration;
using SufiChain.SufiAbp.FileManager.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.BlobStoring;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpFileManagerApplicationContractsModule),
    typeof(SufiAbpAspNetCoreMvcModule),
    typeof(SufiAbpBlobStoringModule))]
public class SufiAbpFileManagerHttpApiModule : AbpModule
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
                .Get<SufiAbpFileManagerResource>()
                .AddBaseTypes(typeof(SufiAbpUiResource));
        });
    }
}
