namespace SufiChain.SufiPlatform.Account;

public static class RegisterFormCheck
{
    public static string? Error(string? email, string? password, string? confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
        {
            return "InvalidEmail";
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            return "ConfirmPasswordMismatch";
        }

        return null;
    }
}
