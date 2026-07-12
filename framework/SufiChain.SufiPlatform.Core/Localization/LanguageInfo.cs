namespace SufiChain.SufiPlatform.Localization;

public class LanguageInfo : Volo.Abp.Localization.LanguageInfo
{
    public LanguageInfo(
        string cultureName,
        string uiCultureName,
        string displayName)
        : base(cultureName, uiCultureName, displayName)
    {
    }
}
