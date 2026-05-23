using SufiChain.SufiAbp.ShortLinkGenerator;
using SufiChain.SufiAbp.ShortLinkGenerator.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.ShortLinkGenerator.Settings;

public class ShortLinkGeneratorSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                ShortLinkGeneratorSettings.BaseUrl,
                defaultValue: string.Empty,
                displayName: L("DisplayName:ShortLinkGenerator.BaseUrl"),
                description: L("Description:ShortLinkGenerator.BaseUrl"),
                isVisibleToClients: true,
                isInherited: true));

        context.Add(
            new SettingDefinition(
                ShortLinkGeneratorSettings.ShortUrl.RedirectRoute,
                defaultValue: ShortLinkGeneratorConsts.DefaultRedirectRoute,
                displayName: L("DisplayName:ShortLinkGenerator.ShortUrl.RedirectRoute"),
                description: L("Description:ShortLinkGenerator.ShortUrl.RedirectRoute"),
                isVisibleToClients: true,
                isInherited: true));

        context.Add(
            new SettingDefinition(
                ShortLinkGeneratorSettings.ShortUrl.ShortCodeLength,
                defaultValue: ShortLinkGeneratorConsts.DefaultShortCodeLength.ToString(),
                displayName: L("DisplayName:ShortLinkGenerator.ShortUrl.ShortCodeLength"),
                description: L("Description:ShortLinkGenerator.ShortUrl.ShortCodeLength"),
                isVisibleToClients: true,
                isInherited: true));

        context.Add(
            new SettingDefinition(
                ShortLinkGeneratorSettings.ShortUrl.CacheExpirationMinutes,
                defaultValue: ShortLinkGeneratorConsts.DefaultCacheExpirationMinutes.ToString(),
                displayName: L("DisplayName:ShortLinkGenerator.ShortUrl.CacheExpirationMinutes"),
                description: L("Description:ShortLinkGenerator.ShortUrl.CacheExpirationMinutes"),
                isVisibleToClients: true,
                isInherited: true));

        context.Add(
            new SettingDefinition(
                ShortLinkGeneratorSettings.ShortUrl.DefaultExpirationDays,
                defaultValue: ShortLinkGeneratorConsts.DefaultExpirationDays.ToString(),
                displayName: L("DisplayName:ShortLinkGenerator.ShortUrl.DefaultExpirationDays"),
                description: L("Description:ShortLinkGenerator.ShortUrl.DefaultExpirationDays"),
                isVisibleToClients: true,
                isInherited: true));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpShortLinkGeneratorResource>(name);
    }
}
