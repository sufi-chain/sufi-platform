using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP;
using SufiChain.SufiPlatform.SufiAI.MCP.IranMcp;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

public class IranMcpTokenVerifyOptionsTests
{
    [Fact]
    public void Setting_definitions_default_to_off()
    {
        var settings = new Dictionary<string, SettingDefinition>();
        var contextType = typeof(SettingDefinition).Assembly.GetType("Volo.Abp.Settings.SettingDefinitionContext");
        Assert.NotNull(contextType);
        var context = (ISettingDefinitionContext)Activator.CreateInstance(contextType!, settings)!;
        new IranMcpSettingDefinitionProvider().Define(context);

        settings[IranMcpSettings.TokenVerifyMode].DefaultValue.ShouldBe("off");
        settings[IranMcpSettings.TokenVerifyUrl].DefaultValue.ShouldBe(string.Empty);
        settings[IranMcpSettings.AllowUpstreamExecution].DefaultValue.ShouldBe("false");
        settings.Keys.ShouldContain(IranMcpSettings.Environment);
    }

    [Theory]
    [InlineData(null, IranMcpTokenVerifyMode.Off)]
    [InlineData("off", IranMcpTokenVerifyMode.Off)]
    [InlineData("optional", IranMcpTokenVerifyMode.Optional)]
    [InlineData("required", IranMcpTokenVerifyMode.Required)]
    public void ParseMode_covers_each_mode(string? value, IranMcpTokenVerifyMode expected)
    {
        IranMcpTokenVerifyOptionsReader.ParseMode(value).ShouldBe(expected);
    }

    [Fact]
    public async Task Off_mode_allows_missing_url()
    {
        var reader = new IranMcpTokenVerifyOptionsReader(
            new FakeSettings(("off", null)),
            new PassthroughLocalizer());

        var options = await reader.GetAsync();
        options.Mode.ShouldBe(IranMcpTokenVerifyMode.Off);
        options.VerifyUrl.ShouldBeNull();
    }

    [Fact]
    public async Task Optional_mode_allows_missing_url()
    {
        var reader = new IranMcpTokenVerifyOptionsReader(
            new FakeSettings(("optional", "  ")),
            new PassthroughLocalizer());

        var options = await reader.GetAsync();
        options.Mode.ShouldBe(IranMcpTokenVerifyMode.Optional);
        options.VerifyUrl.ShouldBeNull();
    }

    [Fact]
    public async Task Required_mode_without_url_fails_closed()
    {
        var reader = new IranMcpTokenVerifyOptionsReader(
            new FakeSettings(("required", null)),
            new PassthroughLocalizer());

        var error = await Should.ThrowAsync<Volo.Abp.BusinessException>(() => reader.GetAsync());
        error.Code.ShouldBe(AIErrorCodes.IranMcpTokenVerifyUrlRequired);
    }

    [Fact]
    public async Task Required_mode_with_url_succeeds()
    {
        var reader = new IranMcpTokenVerifyOptionsReader(
            new FakeSettings(("required", "https://panel.example/api/ai/mcp/user-token/verify")),
            new PassthroughLocalizer());

        var options = await reader.GetAsync();
        options.Mode.ShouldBe(IranMcpTokenVerifyMode.Required);
        options.VerifyUrl.ShouldBe("https://panel.example/api/ai/mcp/user-token/verify");
    }

    private sealed class FakeSettings : ISettingProvider
    {
        private readonly string? _mode;
        private readonly string? _url;

        public FakeSettings((string? Mode, string? Url) values)
        {
            _mode = values.Mode;
            _url = values.Url;
        }

        public Task<string?> GetOrNullAsync(string name)
        {
            if (name == IranMcpSettings.TokenVerifyMode)
            {
                return Task.FromResult(_mode);
            }

            if (name == IranMcpSettings.TokenVerifyUrl)
            {
                return Task.FromResult(_url);
            }

            return Task.FromResult<string?>(null);
        }

        public Task<List<SettingValue>> GetAllAsync(string[] names)
            => Task.FromResult(new List<SettingValue>());

        public Task<List<SettingValue>> GetAllAsync()
            => Task.FromResult(new List<SettingValue>());
    }

    private sealed class PassthroughLocalizer : Microsoft.Extensions.Localization.IStringLocalizer<SufiChain.SufiPlatform.SufiAI.Localization.AIResource>
    {
        public Microsoft.Extensions.Localization.LocalizedString this[string name]
            => new(name, name);

        public Microsoft.Extensions.Localization.LocalizedString this[string name, params object[] arguments]
            => new(name, name);

        public IEnumerable<Microsoft.Extensions.Localization.LocalizedString> GetAllStrings(bool includeParentCultures)
            => Array.Empty<Microsoft.Extensions.Localization.LocalizedString>();
    }
}
