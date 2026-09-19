using System;
using System.Globalization;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.SufiAI.Web;

public static class WebResearchSettings
{
    public const string Prefix = "SufiAI.Hooshvare.WebSearch.";
    public const string Enabled = Prefix + "Enabled";
    public const string Endpoint = Prefix + "Endpoint";
    public const string Token = Prefix + "AuthorizationBearerToken";
    public static readonly (string Name, string Default)[] Limits =
    [ ("SafeSearch", "1"), ("SearchTimeoutSeconds", "8"), ("FetchTimeoutSeconds", "8"),
      ("MaxSearchResults", "5"), ("MaxPagesToFetch", "3"), ("MaxPageBytes", "1048576"),
      ("MaxExtractedCharactersPerPage", "12000"), ("MaxTotalContextCharacters", "30000") ];
}

public class WebResearchSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        Add(context, WebResearchSettings.Enabled, "false");
        Add(context, WebResearchSettings.Endpoint, "");
        Add(context, WebResearchSettings.Token, "", true);
        foreach (var (name, value) in WebResearchSettings.Limits)
            Add(context, WebResearchSettings.Prefix + name, value);
    }

    private static void Add(ISettingDefinitionContext context, string name, string value, bool encrypted = false)
    {
        var setting = new SettingDefinition(name, value, isVisibleToClients: false, isEncrypted: encrypted);
        setting.Providers.Add(GlobalSettingValueProvider.ProviderName);
        setting.Providers.Add(TenantSettingValueProvider.ProviderName);
        context.Add(setting);
    }
}

public sealed class WebResearchOptions
{
    public bool Enabled { get; set; }
    public string Endpoint { get; set; } = "";
    public string? Token { get; set; }
    public int SafeSearch { get; set; } = 1;
    public int SearchTimeoutSeconds { get; set; } = 8;
    public int FetchTimeoutSeconds { get; set; } = 8;
    public int MaxSearchResults { get; set; } = 5;
    public int MaxPagesToFetch { get; set; } = 3;
    public int MaxPageBytes { get; set; } = 1_048_576;
    public int MaxExtractedCharactersPerPage { get; set; } = 12_000;
    public int MaxTotalContextCharacters { get; set; } = 30_000;
}

public class WebResearchOptionsProvider(ISettingProvider settings) : ITransientDependency
{
    public virtual async Task<WebResearchOptions> GetAsync() => new()
    {
        Enabled = bool.TryParse(await settings.GetOrNullAsync(WebResearchSettings.Enabled), out var enabled) && enabled,
        Endpoint = await settings.GetOrNullAsync(WebResearchSettings.Endpoint) ?? "",
        Token = await settings.GetOrNullAsync(WebResearchSettings.Token),
        SafeSearch = await Number("SafeSearch", 1, 0, 2),
        SearchTimeoutSeconds = await Number("SearchTimeoutSeconds", 8, 2, 30),
        FetchTimeoutSeconds = await Number("FetchTimeoutSeconds", 8, 2, 30),
        MaxSearchResults = await Number("MaxSearchResults", 5, 1, 10),
        MaxPagesToFetch = await Number("MaxPagesToFetch", 3, 0, 5),
        MaxPageBytes = await Number("MaxPageBytes", 1_048_576, 1024, 5_242_880),
        MaxExtractedCharactersPerPage = await Number("MaxExtractedCharactersPerPage", 12_000, 100, 24_000),
        MaxTotalContextCharacters = await Number("MaxTotalContextCharacters", 30_000, 1000, 60_000)
    };

    private async Task<int> Number(string name, int fallback, int min, int max) =>
        Math.Clamp(int.TryParse(await settings.GetOrNullAsync(WebResearchSettings.Prefix + name),
            NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback, min, max);
}
