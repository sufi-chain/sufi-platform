using System;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using Volo.Abp;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Decides which user a phone-confirmation call may act for.
/// A session token is proof for the anonymous registration and sign-in flows.
/// A signed-in caller with no token may act only for themselves.
/// </summary>
public static class PhoneConfirmationAccess
{
    public static async Task<Guid> ResolveUserIdAsync(
        ICurrentUser currentUser,
        IPhoneConfirmationSessionStore sessions,
        string? sessionToken)
    {
        if (!string.IsNullOrWhiteSpace(sessionToken))
        {
            var sessionUserId = await sessions.FindUserIdAsync(sessionToken);
            if (!sessionUserId.HasValue)
            {
                throw new BusinessException(IdentitySecurityErrorCodes.PhoneConfirmationSessionInvalid);
            }

            if (currentUser.Id.HasValue && currentUser.Id.Value != sessionUserId.Value)
            {
                throw new BusinessException(IdentitySecurityErrorCodes.PhoneConfirmationSessionInvalid);
            }

            return sessionUserId.Value;
        }

        if (currentUser.Id.HasValue)
        {
            return currentUser.Id.Value;
        }

        throw new BusinessException(IdentitySecurityErrorCodes.PhoneConfirmationSessionInvalid);
    }
}
