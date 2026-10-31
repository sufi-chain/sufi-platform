using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Configuration;
using SufiChain.SufiPlatform.SufiCom.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Configuration;

public class EmailChannelConfigurationAppServiceTests : SufiComTestBase<SufiComApplicationTestModule>
{
    private readonly IEmailChannelConfigurationAppService _configurationAppService;

    public EmailChannelConfigurationAppServiceTests()
    {
        _configurationAppService = GetRequiredService<IEmailChannelConfigurationAppService>();
    }

    [Fact]
    public async Task Should_Get_Default_Configuration()
    {
        var result = await _configurationAppService.GetAsync();

        result.ShouldNotBeNull();
        result.Enabled.ShouldBeFalse();
        result.InboundEnabled.ShouldBeFalse();
        result.UsePlatformSmtp.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Update_Configuration()
    {
        var input = new UpdateEmailChannelConfigurationInput
        {
            Enabled = true,
            UsePlatformSmtp = false,
            DefaultFromAddress = "support@example.com",
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            SmtpUseSsl = true,
            ReplyToAddress = "reply@example.com"
        };

        await WithUnitOfWorkAsync(() => _configurationAppService.UpdateAsync(input));

        var result = await _configurationAppService.GetAsync();
        result.Enabled.ShouldBeTrue();
        result.UsePlatformSmtp.ShouldBeTrue();
        result.ReplyToAddress.ShouldBe("reply@example.com");
        result.SmtpHost.ShouldNotBe("smtp.example.com");
    }

    [Fact]
    public async Task Email_channel_does_not_define_a_second_smtp_path()
    {
        var definitions = await GetRequiredService<Volo.Abp.Settings.ISettingDefinitionManager>().GetAllAsync();

        definitions.ShouldContain(definition => definition.Name == SufiComSettingNames.EmailChannel.Enabled);
        definitions.ShouldContain(definition => definition.Name == SufiComSettingNames.EmailChannel.ReplyToAddress);
        definitions.ShouldNotContain(definition => definition.Name == SufiComSettingNames.EmailChannel.UsePlatformSmtp);
        definitions.ShouldNotContain(definition => definition.Name == SufiComSettingNames.EmailChannel.SmtpHost);
        definitions.ShouldNotContain(definition => definition.Name == SufiComSettingNames.EmailChannel.SmtpPassword);
        definitions.ShouldContain(definition => definition.Name == SufiComSenderSettingNames.Email.SmtpHost);
    }

    [Fact]
    public void Should_Allow_Incomplete_Disabled_Draft()
    {
        var input = new UpdateEmailChannelConfigurationInput
        {
            Enabled = false,
            InboundEnabled = true,
            UsePlatformSmtp = false,
            InboundProtocol = InboundEmailProtocol.Imap
        };

        Validator.TryValidateObject(input, new ValidationContext(input), [], true).ShouldBeTrue();
    }

    [Fact]
    public void Should_Allow_Empty_Connector_From_Address_When_Using_Platform_Smtp()
    {
        var input = new UpdateEmailChannelConfigurationInput
        {
            Enabled = true,
            UsePlatformSmtp = true,
            DefaultFromAddress = string.Empty,
            ReplyToAddress = string.Empty
        };

        Validator.TryValidateObject(input, new ValidationContext(input), [], true).ShouldBeTrue();
    }

    [Fact]
    public void Should_Ignore_Connector_Smtp_Override_Fields()
    {
        var input = new UpdateEmailChannelConfigurationInput
        {
            Enabled = true,
            UsePlatformSmtp = false,
            DefaultFromAddress = "not-an-email",
            SmtpHost = string.Empty,
            SmtpPort = 0
        };

        Validator.TryValidateObject(input, new ValidationContext(input), [], true).ShouldBeTrue();
    }

    [Fact]
    public void Should_Require_Inbound_Fields_When_Inbound_Is_Enabled()
    {
        var input = new UpdateEmailChannelConfigurationInput
        {
            Enabled = true,
            InboundEnabled = true,
            UsePlatformSmtp = false,
            InboundProtocol = InboundEmailProtocol.Imap
        };
        var validationResults = new List<ValidationResult>();

        Validator.TryValidateObject(
            input,
            new ValidationContext(input),
            validationResults,
            true).ShouldBeFalse();

        validationResults.ShouldContain(result => result.MemberNames.Contains(nameof(input.InboundHost)));
        validationResults.ShouldNotContain(result => result.MemberNames.Contains(nameof(input.SmtpHost)));
        validationResults.ShouldNotContain(result => result.MemberNames.Contains(nameof(input.DefaultFromAddress)));
    }

    [Fact]
    public async Task Should_Run_NonDestructive_Inbound_Test_Through_Runtime_Tester()
    {
        FakeEmailChannelConnectionTester.Reset();

        var result = await _configurationAppService.TestInboundAsync();

        result.Success.ShouldBeTrue();
        FakeEmailChannelConnectionTester.InboundTestCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Run_Outbound_Test_Through_Runtime_Tester()
    {
        const string recipient = "operator@example.com";
        FakeEmailChannelConnectionTester.Reset();

        var result = await _configurationAppService.TestOutboundAsync(new TestOutboundEmailChannelInput
        {
            RecipientAddress = recipient
        });

        result.Success.ShouldBeTrue();
        FakeEmailChannelConnectionTester.LastRecipientAddress.ShouldBe(recipient);
    }

    [Fact]
    public async Task Should_Reject_Incomplete_Inbound_Configuration()
    {
        var input = new UpdateEmailChannelConfigurationInput
        {
            Enabled = true,
            InboundEnabled = true,
            InboundProtocol = InboundEmailProtocol.Imap,
            InboundPort = 993,
            InboundUseSsl = true
        };

        var exception = await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => WithUnitOfWorkAsync(() => _configurationAppService.UpdateAsync(input)));

        var invalidMembers = exception.ValidationErrors.SelectMany(error => error.MemberNames).ToList();
        invalidMembers.ShouldContain(nameof(UpdateEmailChannelConfigurationInput.InboundHost));
        invalidMembers.ShouldContain(nameof(UpdateEmailChannelConfigurationInput.InboundUserName));
    }

    [Fact]
    public async Task Should_Update_Explicit_Inbound_Configuration()
    {
        var input = new UpdateEmailChannelConfigurationInput
        {
            Enabled = true,
            InboundEnabled = true,
            InboundProtocol = InboundEmailProtocol.Imap,
            InboundHost = "imap.example.com",
            InboundPort = 993,
            InboundUseSsl = true,
            InboundUserName = "support@example.com",
            InboundPassword = "secret-value"
        };

        await WithUnitOfWorkAsync(() => _configurationAppService.UpdateAsync(input));

        var result = await _configurationAppService.GetAsync();
        result.InboundEnabled.ShouldBeTrue();
        result.InboundProtocol.ShouldBe(InboundEmailProtocol.Imap);
        result.InboundHost.ShouldBe("imap.example.com");
        result.HasInboundPassword.ShouldBeTrue();
    }
}

public class FakeEmailChannelConnectionTester : IEmailChannelConnectionTester, ITransientDependency
{
    public static int InboundTestCount { get; private set; }

    public static string? LastRecipientAddress { get; private set; }

    public static void Reset()
    {
        InboundTestCount = 0;
        LastRecipientAddress = null;
    }

    public Task TestOutboundAsync(string recipientAddress, CancellationToken cancellationToken = default)
    {
        LastRecipientAddress = recipientAddress;
        return Task.CompletedTask;
    }

    public Task TestInboundAsync(CancellationToken cancellationToken = default)
    {
        InboundTestCount++;
        return Task.CompletedTask;
    }
}
