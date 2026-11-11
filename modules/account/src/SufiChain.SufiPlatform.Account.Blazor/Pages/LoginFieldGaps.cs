namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

/// <summary>
/// Which login fields are empty. Username and password are reported separately
/// so a filled username is not asked for again.
/// </summary>
public readonly record struct LoginFieldGaps(bool UserNameOrEmail, bool Password)
{
    public bool Any => UserNameOrEmail || Password;

    public static LoginFieldGaps From(string? userNameOrEmail, string? password)
    {
        return new LoginFieldGaps(
            string.IsNullOrWhiteSpace(userNameOrEmail),
            string.IsNullOrWhiteSpace(password));
    }
}
