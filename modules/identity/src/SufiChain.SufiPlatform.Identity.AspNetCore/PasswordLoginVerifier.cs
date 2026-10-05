using Microsoft.AspNetCore.Identity;

namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Verifies an Identity password hash without writing the user.
/// <see cref="UserManager{TUser}.CheckPasswordAsync"/> rehashes and updates the user when the
/// stored hash is an older format. That update throws for a detached user graph or a concurrency
/// conflict, and the login page was turning the exception into a generic failure.
/// </summary>
public static class PasswordLoginVerifier
{
    public static PasswordVerificationResult Verify(
        IPasswordHasher<IdentityUser> passwordHasher,
        IdentityUser user,
        string password,
        out bool hashFormatInvalid)
    {
        hashFormatInvalid = false;
        if (passwordHasher == null || user == null || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(user.PasswordHash))
        {
            return PasswordVerificationResult.Failed;
        }

        try
        {
            return passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        }
        catch (Exception exception) when (exception is FormatException or ArgumentException)
        {
            hashFormatInvalid = true;
            return PasswordVerificationResult.Failed;
        }
    }
}
