using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Configuration;
using Volo.Abp.Data;
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
}
