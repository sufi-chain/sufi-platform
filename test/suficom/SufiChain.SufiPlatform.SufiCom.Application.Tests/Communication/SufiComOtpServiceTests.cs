using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Channels;
using SufiChain.SufiPlatform.SufiCom.Communication;
using SufiChain.SufiPlatform.SufiCom.Communication.Repositories;
using SufiChain.SufiPlatform.SufiCom.Configuration;
using SufiChain.SufiPlatform.SufiCom.Localization;
using Volo.Abp.Data;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Communication;

public class SufiComOtpServiceTests : SufiComTestBase<SufiComApplicationTestModule>
{
    private readonly SufiComOtpService _otpService;
    private readonly ISmsProviderConfigurationAppService _configurationAppService;
    private readonly IChannelMessageAuditRepository _auditRepository;
    private readonly IStringLocalizer<SufiComResource> _localizer;

    public SufiComOtpServiceTests()
    {
        _otpService = GetRequiredService<SufiComOtpService>();
        _configurationAppService = GetRequiredService<ISmsProviderConfigurationAppService>();
        _auditRepository = GetRequiredService<IChannelMessageAuditRepository>();
        _localizer = GetRequiredService<IStringLocalizer<SufiComResource>>();
    }

    [Theory]
    [InlineData("Your code is 123456", "123456", "Your code is ******")]
    [InlineData("123456 is your code. Repeat: 123456", "123456", "****** is your code. Repeat: ******")]
    [InlineData("No code here", "", "No code here")]
    public void MaskCode_Should_Replace_Every_Occurrence(string content, string code, string expected)
    {
        SufiComOtpService.MaskCode(content, code).ShouldBe(expected);
    }

    [Fact]
    public async Task SendSms_Should_Fail_Without_Plain_Text_Fallback_When_The_Otp_Template_Is_Missing()
    {
        await WithUnitOfWorkAsync(() => _configurationAppService.UpdateConfigurationAsync(
            new UpdateSmsProviderConfigurationDto
            {
                ProviderCode = "Kavenegar",
                IsEnabled = true,
                ExtraProperties = new ExtraPropertyDictionary
                {
                    ["ApiKey"] = "test-key",
                    ["SenderNumber"] = "1000"
                }
            }));

        var auditId = await _otpService.SendSmsAsync(new OtpMessage
        {
            Phone = "+989121234567",
            Code = "123456",
            Purpose = OtpPurposes.Login,
            Content = "Your code is 123456",
            IdempotencyKey = "Login:+989121234567:1"
        });

        var audit = await WithUnitOfWorkAsync(() => _auditRepository.GetAsync(auditId));
        audit.State.ShouldBe(ChannelMessageAuditState.Failed);
        audit.ProviderCode.ShouldBe("Kavenegar");
        audit.RenderedContent.ShouldBe("Your code is ******");
        audit.FailureReason.ShouldBe(
            _localizer[SufiComDomainErrorCodes.OtpRouteNotConfigured].Value + " " +
            _localizer[OtpProviderSettingKeys.TemplateRequiredError].Value);
    }

    [Theory]
    [InlineData(OtpFailureMessages.KavenegarRejected, "418", "Kavenegar", "کاوه")]
    [InlineData(OtpFailureMessages.SmsIrRejected, "400", "SMS.ir", "SMS.ir")]
    [InlineData(OtpFailureMessages.FanapRejected, "26", "Fanap", "فناپ")]
    [InlineData(OtpFailureMessages.TimedOut, "", "timeout", "مهلت")]
    [InlineData(OtpFailureMessages.InvalidPhone, "", "Iranian", "ایرانی")]
    public void Failure_Text_Should_Localize_In_English_And_Persian(
        string key,
        string status,
        string englishWord,
        string persianWord)
    {
        var coded = string.IsNullOrEmpty(status)
            ? key
            : key == OtpFailureMessages.SmsIrRejected
                ? OtpFailureMessages.Format(key, 400, 0)
                : OtpFailureMessages.Format(key, status);
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var english = OtpFailureMessages.Localize(_localizer, coded);
            english.ShouldContain(englishWord);
            english.ShouldNotContain(key);

            CultureInfo.CurrentUICulture = new CultureInfo("fa");
            var persian = OtpFailureMessages.Localize(_localizer, coded);
            persian.ShouldContain(persianWord);
            persian.ShouldNotContain(key);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}
