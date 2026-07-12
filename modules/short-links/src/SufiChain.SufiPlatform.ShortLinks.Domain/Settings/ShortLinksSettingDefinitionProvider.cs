using SufiChain.SufiPlatform.ShortLinks;
using SufiChain.SufiPlatform.ShortLinks.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.ShortLinks.Settings;

public class ShortLinksSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                ShortLinksSettings.BaseUrl,
                defaultValue: string.Empty,
                displayName: L("DisplayName:ShortLinks.BaseUrl"),
                description: L("Description:ShortLinks.BaseUrl"),
                isVisibleToClients: true,
                isInherited: true));

        context.Add(
            new SettingDefinition(
                ShortLinksSettings.ShortUrl.RedirectRoute,
                defaultValue: ShortLinksConsts.DefaultRedirectRoute,
                displayName: L("DisplayName:ShortLinks.ShortUrl.RedirectRoute"),
                description: L("Description:ShortLinks.ShortUrl.RedirectRoute"),
                isVisibleToClients: true,
                isInherited: true));

        context.Add(
            new SettingDefinition(
                ShortLinksSettings.ShortUrl.ShortCodeLength,
                defaultValue: ShortLinksConsts.DefaultShortCodeLength.ToString(),
                displayName: L("DisplayName:ShortLinks.ShortUrl.ShortCodeLength"),
                description: L("Description:ShortLinks.ShortUrl.ShortCodeLength"),
                isVisibleToClients: true,
                isInherited: true));

        context.Add(
            new SettingDefinition(
                ShortLinksSettings.ShortUrl.CacheExpirationMinutes,
                defaultValue: ShortLinksConsts.DefaultCacheExpirationMinutes.ToString(),
                displayName: L("DisplayName:ShortLinks.ShortUrl.CacheExpirationMinutes"),
                description: L("Description:ShortLinks.ShortUrl.CacheExpirationMinutes"),
                isVisibleToClients: true,
                isInherited: true));

        context.Add(
            new SettingDefinition(
                ShortLinksSettings.ShortUrl.DefaultExpirationDays,
                defaultValue: ShortLinksConsts.DefaultExpirationDays.ToString(),
                displayName: L("DisplayName:ShortLinks.ShortUrl.DefaultExpirationDays"),
                description: L("Description:ShortLinks.ShortUrl.DefaultExpirationDays"),
                isVisibleToClients: true,
                isInherited: true));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiShortLinksResource>(name);
    }
}