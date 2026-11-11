using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.SufiCom.Configuration;
using SufiChain.SufiPlatform.SufiCom.Settings;
using Volo.Abp.Data;
using Volo.Abp.Validation;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Configuration;

public class VoiceProviderConfigurationAppServiceTests : SufiComTestBase<SufiComApplicationTestModule>
{
    private readonly IVoiceProviderConfigurationAppService _configurationAppService;

    public VoiceProviderConfigurationAppServiceTests()
    {
        _configurationAppService = GetRequiredService<IVoiceProviderConfigurationAppService>();
    }

    [Fact]
    public async Task Should_Return_Null_When_No_Configuration()
    {
        var result = await _configurationAppService.GetConfigurationAsync();

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Update_And_Get_Configuration()
    {
        var input = new UpdateVoiceProviderConfigurationDto
        {
            ProviderCode = "Kavenegar",
            IsEnabled = false,
            ExtraProperties = new ExtraPropertyDictionary
            {
                ["ApiKey"] = "test-key"
            }
        };

        var result = await WithUnitOfWorkAsync(() => _configurationAppService.UpdateConfigurationAsync(input));

        result.ProviderCode.ShouldBe("Kavenegar");
        result.IsEnabled.ShouldBeFalse();

        var saved = await _configurationAppService.GetConfigurationAsync();
        saved.ShouldNotBeNull();
        saved!.ProviderCode.ShouldBe("Kavenegar");
        saved.IsEnabled.ShouldBeFalse();
        saved.HasStoredSensitiveSettings.ShouldBeTrue();
        saved.ExtraProperties.ContainsKey("ApiKey").ShouldBeFalse();
    }

    [Fact]
    public async Task Should_List_Available_Providers()
    {
        var providers = await _configurationAppService.GetAvailableProvidersAsync();

        providers.ShouldNotBeEmpty();
        providers.ShouldContain(p => p.ProviderCode == "Kavenegar");
    }

    [Fact]
    public async Task Should_Disable_Voice_When_No_Provider_Is_Selected()
    {
        var manager = GetRequiredService<ISettingManager>();
        await manager.SetAsync(SufiComSettingNames.VoiceChannel.Enabled, "true");

        var loaded = await _configurationAppService.GetConfigurationAsync();
        loaded.ShouldNotBeNull();
        loaded!.IsEnabled.ShouldBeTrue();
        loaded.ProviderCode.ShouldBeNullOrEmpty();

        var saved = await WithUnitOfWorkAsync(() => _configurationAppService.UpdateConfigurationAsync(
            new UpdateVoiceProviderConfigurationDto { IsEnabled = false }));

        saved.IsEnabled.ShouldBeFalse();
        (await _configurationAppService.GetConfigurationAsync()).ShouldBeNull();
    }

    [Fact]
    public async Task Should_Require_A_Provider_Only_When_Enabling_Voice()
    {
        var exception = await Should.ThrowAsync<AbpValidationException>(() =>
            WithUnitOfWorkAsync(() => _configurationAppService.UpdateConfigurationAsync(
                new UpdateVoiceProviderConfigurationDto { IsEnabled = true })));

        exception.ValidationErrors.ShouldContain(error =>
            error.MemberNames.Contains(nameof(UpdateVoiceProviderConfigurationDto.ProviderCode)));
    }
}
