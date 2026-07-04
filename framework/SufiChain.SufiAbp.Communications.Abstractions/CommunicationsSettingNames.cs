namespace SufiChain.SufiAbp.Communications;

public static class CommunicationsSettingNames
{
    public const string GroupName = "SufiAbp.Communications";

    public static class Email
    {
        private const string Prefix = GroupName + ".Email";
        
        public const string DefaultFromAddress = Prefix + ".DefaultFromAddress";
        public const string DefaultFromDisplayName = Prefix + ".DefaultFromDisplayName";
        public const string SmtpHost = Prefix + ".Smtp.Host";
        public const string SmtpPort = Prefix + ".Smtp.Port";
        public const string SmtpUserName = Prefix + ".Smtp.UserName";
        public const string SmtpPassword = Prefix + ".Smtp.Password";
        public const string SmtpDomain = Prefix + ".Smtp.Domain";
        public const string SmtpEnableSsl = Prefix + ".Smtp.EnableSsl";
        public const string SmtpUseDefaultCredentials = Prefix + ".Smtp.UseDefaultCredentials";
    }

    public static class Sms
    {
        private const string Prefix = GroupName + ".Sms";
        
        public const string DefaultFromNumber = Prefix + ".DefaultFromNumber";
        public const string ProviderName = Prefix + ".ProviderName";
    }

    public static class VoiceCall
    {
        private const string Prefix = GroupName + ".VoiceCall";
        
        public const string DefaultFromNumber = Prefix + ".DefaultFromNumber";
        public const string DefaultLanguage = Prefix + ".DefaultLanguage";
        public const string DefaultVoiceGender = Prefix + ".DefaultVoiceGender";
        public const string ProviderName = Prefix + ".ProviderName";
    }
}
