using Microsoft.AspNetCore.Identity;

namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Outcome of a password sign-in attempt before a cookie is issued.
/// </summary>
public enum PasswordLoginKind
{
    Success = 0,
    InvalidCredentials,
    LockedOut,
    Inactive,
    EmailConfirmationRequired,
    PhoneConfirmationRequired,
    ExternalLoginOnly,
    TenantMismatch,
    TwoFactorRequired
}

/// <summary>
/// Facts the login page already knows. The decision does not read settings or the database.
/// </summary>
public readonly record struct PasswordLoginFacts(
    bool UserFound,
    bool TenantSelected,
    bool IsActive,
    bool IsLockedOut,
    bool HasPassword,
    bool IsExternal,
    bool PasswordAccepted,
    bool EmailConfirmed,
    bool RequireConfirmedEmail,
    bool CanSignIn,
    bool PhoneConfirmationRequired,
    bool PhoneNumberConfirmed,
    bool TwoFactorEnabled);

/// <summary>
/// Maps a password sign-in to a specific result so the page does not collapse every failure
/// into one message.
/// </summary>
public static class PasswordLoginDecision
{
    public static PasswordLoginKind Decide(PasswordLoginFacts facts)
    {
        if (!facts.UserFound)
        {
            return facts.TenantSelected
                ? PasswordLoginKind.TenantMismatch
                : PasswordLoginKind.InvalidCredentials;
        }

        if (facts.IsLockedOut)
        {
            return PasswordLoginKind.LockedOut;
        }

        if (!facts.HasPassword && facts.IsExternal)
        {
            return PasswordLoginKind.ExternalLoginOnly;
        }

        if (!facts.PasswordAccepted)
        {
            return PasswordLoginKind.InvalidCredentials;
        }

        if (!facts.IsActive)
        {
            return PasswordLoginKind.Inactive;
        }

        if ((facts.RequireConfirmedEmail && !facts.EmailConfirmed) ||
            (!facts.CanSignIn && !facts.EmailConfirmed))
        {
            return PasswordLoginKind.EmailConfirmationRequired;
        }

        if (!facts.CanSignIn)
        {
            return PasswordLoginKind.Inactive;
        }

        if (facts.PhoneConfirmationRequired && !facts.PhoneNumberConfirmed)
        {
            return PasswordLoginKind.PhoneConfirmationRequired;
        }

        if (facts.TwoFactorEnabled)
        {
            return PasswordLoginKind.TwoFactorRequired;
        }

        return PasswordLoginKind.Success;
    }

    public static string LocalizationKey(PasswordLoginKind kind)
    {
        return kind switch
        {
            PasswordLoginKind.InvalidCredentials => "InvalidCredentials",
            PasswordLoginKind.LockedOut => "AccountLockedOut",
            PasswordLoginKind.Inactive => "LoginAccountInactive",
            PasswordLoginKind.EmailConfirmationRequired => "Sufi.Identity:EmailConfirmationRequired",
            PasswordLoginKind.PhoneConfirmationRequired => "LoginPhoneConfirmationRequired",
            PasswordLoginKind.ExternalLoginOnly => "LoginExternalAccountOnly",
            PasswordLoginKind.TenantMismatch => "LoginTenantMismatch",
            PasswordLoginKind.TwoFactorRequired => "LoginRequiresTwoFactor",
            _ => "LoginUnexpectedError"
        };
    }

    /// <summary>
    /// A successful Identity verification includes hashes that should be upgraded.
    /// The upgrade write is optional; a failed write must not reject the password.
    /// </summary>
    public static bool IsPasswordAccepted(PasswordVerificationResult result)
    {
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
