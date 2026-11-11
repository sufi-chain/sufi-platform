using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.Features;
using RequiresFeatureAttribute = SufiChain.SufiPlatform.Features.RequiresFeatureAttribute;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileMigration;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.Settings;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp.Authorization;
using Volo.Abp.Features;
using Volo.Abp.Modularity;
using Xunit;
using AbpFeatureChecker = Volo.Abp.Features.IFeatureChecker;

namespace SufiChain.SufiPlatform.FileManager;

public class FileManagerFeatureAttributeTests
{
    [Fact]
    public void App_services_declare_the_feature_that_gates_them()
    {
        Required(typeof(FileItemAppService)).ShouldContain(SufiFileManagerFeatures.Enable);
        Required(typeof(FileItemAppService)).ShouldContain(SufiFileManagerFeatures.FileItems);
        Required(typeof(FileStructureAppService)).ShouldContain(SufiFileManagerFeatures.FileStructures);
        Required(typeof(FileManagerStorageSettingsAppService)).ShouldContain(SufiFileManagerFeatures.StorageSettings);
        Required(typeof(FileArchivingSettingsAppService)).ShouldContain(SufiFileManagerFeatures.Archiving);
        Required(typeof(FileMigrationAppService)).ShouldContain(SufiFileManagerFeatures.Enable);
        Required(typeof(FileMigrationAppService)).ShouldContain(SufiFileManagerFeatures.StorageSettings);
        Required(typeof(FileManagerSettingsAppService)).ShouldContain(SufiFileManagerFeatures.Enable);
    }

    private static string[] Required(Type service)
    {
        return service.GetCustomAttributes(inherit: true)
            .OfType<RequiresFeatureAttribute>()
            .SelectMany(attribute => attribute.Features)
            .ToArray();
    }
}

[DependsOn(typeof(SufiFileManagerApplicationTestModule))]
public class FileManagerFeatureDisabledTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Transient<AbpFeatureChecker, FileManagerOffFeatureChecker>());
    }
}

public class FileManagerOffFeatureChecker : AbpFeatureChecker
{
    public Task<string> GetOrNullAsync(string name)
    {
        return Task.FromResult(IsFileManager(name) ? "false" : "true");
    }

    public Task<bool> IsEnabledAsync(string name)
    {
        return Task.FromResult(!IsFileManager(name));
    }

    public Task<Dictionary<string, bool>> IsEnabledAsync(string[] names)
    {
        return Task.FromResult(names.ToDictionary(name => name, name => !IsFileManager(name)));
    }

    private static bool IsFileManager(string name)
    {
        return name.StartsWith(SufiFileManagerFeatures.GroupName, StringComparison.Ordinal);
    }
}

public class FileManagerFeatureGateTests : FileManagerApplicationTestBase<FileManagerFeatureDisabledTestModule>
{
    [Fact]
    public async Task File_item_service_throws_when_the_feature_is_off()
    {
        var service = GetRequiredService<IFileItemAppService>();

        var exception = await Should.ThrowAsync<AbpAuthorizationException>(
            () => service.GetAsync(Guid.NewGuid()));

        exception.Code.ShouldNotBeNull();
        exception.Code.ShouldStartWith("Volo.Feature:");
    }
}
