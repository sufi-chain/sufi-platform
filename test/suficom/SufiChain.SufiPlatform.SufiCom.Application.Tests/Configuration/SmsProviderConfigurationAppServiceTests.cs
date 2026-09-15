using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Configuration;
using SufiChain.SufiPlatform.SufiCom.Settings;
using Volo.Abp.Data;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Configuration;

public class SmsProviderConfigurationAppServiceTests : SufiComTestBase<SufiComApplicationTestModule>
{
    private const string ProviderA = "Kavenegar";
    private const string ProviderASender = "1000";
    private const string ProviderBSender = "B-SENDER-9999";
    private const string TestPhone = "09123456789";

    private readonly ISmsProviderConfigurationAppService _configurationAppService;
    private readonly ISettingProvider _settingProvider;

    public SmsProviderConfigurationAppServiceTests()
    {
        _configurationAppService = GetRequiredService<ISmsProviderConfigurationAppService>();
        _settingProvider = GetRequiredService<ISettingProvider>();
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
        var input = new UpdateSmsProviderConfigurationDto
        {
            ProviderCode = "Kavenegar",
            IsEnabled = false,
            ExtraProperties = new ExtraPropertyDictionary
            {
                ["ApiKey"] = "test-key",
                ["SenderNumber"] = "1000"
            }
        };

        var result = await WithUnitOfWorkAsync(() => _configurationAppService.UpdateConfigurationAsync(input));

        result.ProviderCode.ShouldBe("Kavenegar");
        result.IsEnabled.ShouldBeFalse();

        var saved = await WithUnitOfWorkAsync(() => _configurationAppService.GetConfigurationAsync());
        saved.ShouldNotBeNull();
        saved!.ProviderCode.ShouldBe("Kavenegar");
        saved.IsEnabled.ShouldBeFalse();
        saved.ExtraProperties["SenderNumber"].ShouldBe("1000");
    }

    [Fact]
    public async Task Should_List_Available_Providers()
    {
        var providers = await _configurationAppService.GetAvailableProvidersAsync();

        providers.ShouldNotBeEmpty();
        providers.ShouldContain(p => p.ProviderCode == ProviderA);
    }

    [Fact]
    public async Task TestProvider_Should_Not_Persist_Provider_B_When_Provider_A_Is_Saved()
    {
        await PersistProviderAAsync();

        var providerB = await ResolveProviderBCodeAsync();
        var testResult = await _configurationAppService.TestProviderAsync(CreateProviderBTestInput(providerB));

        testResult.ShouldNotBeNull();
        if (providerB == "MissingSmsProvider")
        {
            testResult.Success.ShouldBeFalse();
            testResult.Message.ShouldContain(providerB);
        }
        else
        {
            testResult.Success.ShouldBeFalse();
        }

        await AssertPersistedProviderAUnchangedAsync();
    }

    [Fact]
    public async Task TestProvider_Unknown_Provider_Should_Fail_Without_Mutating_Saved_Configuration()
    {
        await PersistProviderAAsync();

        var testResult = await _configurationAppService.TestProviderAsync(CreateProviderBTestInput("MissingSmsProvider"));

        testResult.Success.ShouldBeFalse();
        testResult.Message.ShouldContain("MissingSmsProvider");

        await AssertPersistedProviderAUnchangedAsync();
    }

    [Fact]
    public async Task TestProvider_Invalid_Provider_B_Settings_Should_Fail_Without_Mutating_Saved_Configuration()
    {
        await PersistProviderAAsync();

        var providerB = await ResolveProviderBCodeAsync();
        var testResult = await _configurationAppService.TestProviderAsync(new TestSmsProviderDto
        {
            ProviderCode = providerB,
            TestPhoneNumber = TestPhone,
            TestMessage = "invalid-b-health-check",
            ExtraProperties = new ExtraPropertyDictionary
            {
                ["SenderNumber"] = ProviderBSender
            }
        });

        testResult.Success.ShouldBeFalse();
        await AssertPersistedProviderAUnchangedAsync();
    }

    private async Task PersistProviderAAsync()
    {
        await WithUnitOfWorkAsync(() => _configurationAppService.UpdateConfigurationAsync(
            new UpdateSmsProviderConfigurationDto
            {
                ProviderCode = ProviderA,
                IsEnabled = false,
                ExtraProperties = new ExtraPropertyDictionary
                {
                    ["ApiKey"] = "test-key-a",
                    ["SenderNumber"] = ProviderASender
                }
            }));
    }

    private async Task<string> ResolveProviderBCodeAsync()
    {
        var providers = await _configurationAppService.GetAvailableProvidersAsync();
        return providers.FirstOrDefault(provider => provider.ProviderCode != ProviderA)?.ProviderCode
               ?? "MissingSmsProvider";
    }

    private static TestSmsProviderDto CreateProviderBTestInput(string providerB)
    {
        return new TestSmsProviderDto
        {
            ProviderCode = providerB,
            TestPhoneNumber = TestPhone,
            TestMessage = "beta-5-provider-b",
            ExtraProperties = new ExtraPropertyDictionary
            {
                ["SenderNumber"] = ProviderBSender
            }
        };
    }

    private async Task AssertPersistedProviderAUnchangedAsync()
    {
        var saved = await _configurationAppService.GetConfigurationAsync();
        saved.ShouldNotBeNull();
        saved!.ProviderCode.ShouldBe(ProviderA);
        saved.IsEnabled.ShouldBeFalse();
        saved.ExtraProperties["SenderNumber"].ShouldBe(ProviderASender);
        saved.ExtraProperties.ContainsKey("ApiKey").ShouldBeFalse();

        var persistedCode = await WithUnitOfWorkAsync(() =>
            _settingProvider.GetOrNullAsync(SufiComSettingNames.SmsChannel.ProviderCode));
        persistedCode.ShouldBe(ProviderA);

        var persistedSender = await WithUnitOfWorkAsync(() =>
            _settingProvider.GetOrNullAsync(SufiComSettingNames.SmsChannel.Setting("SenderNumber")));
        persistedSender.ShouldBe(ProviderASender);
    }
}
