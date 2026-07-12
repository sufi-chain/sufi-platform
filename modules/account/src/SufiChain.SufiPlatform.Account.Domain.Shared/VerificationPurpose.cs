namespace SufiChain.SufiPlatform.Account;

public enum VerificationPurpose
{
    OtpLogin = 0,
    OtpRegistration = 1,
    TwoFactorCode = 2,
    EmailConfirmation = 3,
    PasswordReset = 4
}
