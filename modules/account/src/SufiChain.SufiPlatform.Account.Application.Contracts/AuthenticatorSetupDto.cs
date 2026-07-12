namespace SufiChain.SufiPlatform.Account;

public class AuthenticatorSetupDto
{
    public string SharedKey { get; set; } = string.Empty;

    public string AuthenticatorUri { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;
}
