using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Account.Verification;

public class MaskIdentifierTests
{
    [Fact]
    public void MaskIdentifier_masks_iranian_mobile()
    {
        VerificationIdentifierHelper
            .MaskIdentifier(VerificationDeliveryChannel.Sms, "+989120322854")
            .ShouldBe("+98912***2854");
    }

    [Fact]
    public void MaskIdentifier_masks_short_phone()
    {
        VerificationIdentifierHelper
            .MaskIdentifier(VerificationDeliveryChannel.Sms, "1234")
            .ShouldBe("***");
    }

    [Fact]
    public void MaskIdentifier_masks_email()
    {
        VerificationIdentifierHelper
            .MaskIdentifier(VerificationDeliveryChannel.Email, "pooria@example.com")
            .ShouldBe("p***@example.com");
    }

    [Fact]
    public void MaskIdentifier_does_not_include_full_phone_digits()
    {
        var masked = VerificationIdentifierHelper.MaskIdentifier(
            VerificationDeliveryChannel.Sms,
            "09120322854");

        masked.ShouldNotContain("09120322854");
        masked.ShouldNotContain("9120322854");
        masked.ShouldContain("***");
    }
}
