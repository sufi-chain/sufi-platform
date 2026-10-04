using SufiChain.SufiPlatform.Identity;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Registration response. <see cref="PhoneConfirmationToken"/> is set when the new user still needs to confirm a phone.
/// Callers pass that token to the phone confirmation API. They do not pass the user id.
/// </summary>
public class AccountRegistrationResultDto : IdentityUserDto
{
    public string? PhoneConfirmationToken { get; set; }
}
