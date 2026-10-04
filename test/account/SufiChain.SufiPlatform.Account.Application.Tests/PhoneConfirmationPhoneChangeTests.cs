using Shouldly;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class PhoneConfirmationPhoneChangeTests
{
    [Fact]
    public void Unconfirmed_Number_Can_Be_Replaced()
    {
        var user = NewUser();
        user.SetPhoneNumber("+989121234567", confirmed: false);

        PhoneConfirmationPhoneChange.EnsureCanAssign(user, "+989121234568");
    }

    [Fact]
    public void Confirmed_Number_Can_Be_Sent_Again()
    {
        var user = NewUser();
        user.SetPhoneNumber("+989121234567", confirmed: true);

        PhoneConfirmationPhoneChange.EnsureCanAssign(user, "+989121234567");
    }

    [Fact]
    public void Confirmed_Number_Cannot_Be_Replaced()
    {
        var user = NewUser();
        user.SetPhoneNumber("+989121234567", confirmed: true);

        var exception = Should.Throw<BusinessException>(() =>
            PhoneConfirmationPhoneChange.EnsureCanAssign(user, "+989121234568"));

        exception.Code.ShouldBe(IdentitySecurityErrorCodes.PhoneNumberAlreadyConfirmed);
    }

    private static IdentityUser NewUser()
    {
        return new IdentityUser(Guid.NewGuid(), "user", "user@example.com");
    }
}
