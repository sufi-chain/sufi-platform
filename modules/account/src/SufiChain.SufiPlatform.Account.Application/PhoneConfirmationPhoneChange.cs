using System;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Phone confirmation may set or correct an unconfirmed number.
/// A confirmed number stays on the profile update path.
/// </summary>
public static class PhoneConfirmationPhoneChange
{
    public static void EnsureCanAssign(IdentityUser user, string phone)
    {
        if (user.PhoneNumberConfirmed &&
            !string.Equals(user.PhoneNumber, phone, StringComparison.Ordinal))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.PhoneNumberAlreadyConfirmed);
        }
    }
}
