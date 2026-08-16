namespace SufiChain.SufiPlatform.Settings;

/// <summary>
/// Compatibility aliases for the platform email sender settings.
/// New code should use SufiComSenderSettingNames from the SufiCom abstractions package.
/// </summary>
[Obsolete("Use SufiComSenderSettingNames.Email instead.")]
public static class EmailSettingNames
{
    public const string DefaultFromAddress = "SufiCom.Email.DefaultFromAddress";
    public const string DefaultFromDisplayName = "SufiCom.Email.DefaultFromDisplayName";

    public static class Smtp
    {
        public const string Host = "SufiCom.Email.Smtp.Host";
        public const string Port = "SufiCom.Email.Smtp.Port";
        public const string UserName = "SufiCom.Email.Smtp.UserName";
        public const string Password = "SufiCom.Email.Smtp.Password";
        public const string Domain = "SufiCom.Email.Smtp.Domain";
        public const string EnableSsl = "SufiCom.Email.Smtp.EnableSsl";
        public const string UseDefaultCredentials = "SufiCom.Email.Smtp.UseDefaultCredentials";
    }
}
