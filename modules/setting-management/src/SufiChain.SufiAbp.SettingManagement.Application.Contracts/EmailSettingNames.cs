namespace SufiChain.SufiAbp.SettingManagement;

public static class EmailSettingNames
{
    public const string DefaultFromAddress = "Abp.Mailing.DefaultFromAddress";
    public const string DefaultFromDisplayName = "Abp.Mailing.DefaultFromDisplayName";

    public static class Smtp
    {
        public const string Host = "Abp.Mailing.Smtp.Host";
        public const string Port = "Abp.Mailing.Smtp.Port";
        public const string UserName = "Abp.Mailing.Smtp.UserName";
        public const string Password = "Abp.Mailing.Smtp.Password";
        public const string Domain = "Abp.Mailing.Smtp.Domain";
        public const string EnableSsl = "Abp.Mailing.Smtp.EnableSsl";
        public const string UseDefaultCredentials = "Abp.Mailing.Smtp.UseDefaultCredentials";
    }
}
