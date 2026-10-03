using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Otp;

public class OtpNormalizationTests
{
    [Theory]
    [InlineData("09121234567")]
    [InlineData("+989121234567")]
    [InlineData("00989121234567")]
    [InlineData("989121234567")]
    [InlineData("9121234567")]
    [InlineData("+98 912 123 4567")]
    [InlineData("۰۹۱۲۱۲۳۴۵۶۷")]
    [InlineData("٠٩١٢١٢٣٤٥٦٧")]
    public void ToNationalNumber_Should_Accept_Every_Iranian_Mobile_Format(string phone)
    {
        IranianMobileNumber.ToNationalNumber(phone).ShouldBe("9121234567");
        IranianMobileNumber.ToLocal(phone).ShouldBe("09121234567");
        IranianMobileNumber.ToInternational(phone).ShouldBe("+989121234567");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("02112345678")]
    [InlineData("+14155550100")]
    public void ToNationalNumber_Should_Reject_Other_Numbers(string? phone)
    {
        IranianMobileNumber.ToNationalNumber(phone).ShouldBeNull();
    }

    [Fact]
    public void ToInternational_Should_Keep_Foreign_Numbers()
    {
        IranianMobileNumber.ToInternational(" +14155550100 ").ShouldBe("+14155550100");
    }

    [Fact]
    public void ResolveTemplate_Should_Prefer_The_Purpose_Template()
    {
        var settings = new Dictionary<string, string>
        {
            [OtpProviderSettingKeys.Template] = " verify ",
            [OtpProviderSettingKeys.TemplateTwoFactor] = "two-factor",
            [OtpProviderSettingKeys.TemplateLogin] = " "
        };

        OtpProviderSettingKeys.ResolveTemplate(settings, OtpPurposes.TwoFactor).ShouldBe("two-factor");
        OtpProviderSettingKeys.ResolveTemplate(settings, OtpPurposes.Login).ShouldBe("verify");
        OtpProviderSettingKeys.ResolveTemplate(settings, OtpPurposes.Verification).ShouldBe("verify");
    }

    [Fact]
    public void ResolveTemplate_Should_Return_Null_Without_Any_Template()
    {
        OtpProviderSettingKeys.ResolveTemplate(new Dictionary<string, string>(), OtpPurposes.Login).ShouldBeNull();
    }
}
