using System.Globalization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.SufiAI;

public interface ISharedWorkspaceRoutePriceSource
{
    Task<SharedWorkspaceRoutePriceSchedule> GetAsync();
}

[ExposeServices(typeof(ISharedWorkspaceRoutePriceSource))]
public class SharedWorkspaceRoutePriceReader : ISharedWorkspaceRoutePriceSource, ITransientDependency
{
    private readonly ISettingProvider _settings;

    public SharedWorkspaceRoutePriceReader(ISettingProvider settings)
    {
        _settings = settings;
    }

    public virtual async Task<SharedWorkspaceRoutePriceSchedule> GetAsync()
    {
        return new SharedWorkspaceRoutePriceSchedule
        {
            ChatInputPerMillionTokens = await PriceAsync(SharedWorkspaceRoutePriceSettings.ChatInputPerMillionTokens),
            ChatOutputPerMillionTokens = await PriceAsync(SharedWorkspaceRoutePriceSettings.ChatOutputPerMillionTokens),
            EmbeddingsInputPerMillionTokens = await PriceAsync(SharedWorkspaceRoutePriceSettings.EmbeddingsInputPerMillionTokens),
            AudioPerMinute = await PriceAsync(SharedWorkspaceRoutePriceSettings.AudioPerMinute),
            SpeechPerMillionCharacters = await PriceAsync(SharedWorkspaceRoutePriceSettings.SpeechPerMillionCharacters),
            VisionInputPerMillionTokens = await PriceAsync(SharedWorkspaceRoutePriceSettings.VisionInputPerMillionTokens),
            VisionOutputPerMillionTokens = await PriceAsync(SharedWorkspaceRoutePriceSettings.VisionOutputPerMillionTokens),
            ImagePerImage = await PriceAsync(SharedWorkspaceRoutePriceSettings.ImagePerImage),
            WebPerRequest = await PriceAsync(SharedWorkspaceRoutePriceSettings.WebPerRequest),
            DecisionsInputPerMillionTokens = await PriceAsync(SharedWorkspaceRoutePriceSettings.DecisionsInputPerMillionTokens),
            DecisionsOutputPerMillionTokens = await PriceAsync(SharedWorkspaceRoutePriceSettings.DecisionsOutputPerMillionTokens)
        };
    }

    private async Task<decimal?> PriceAsync(string name)
    {
        var raw = await _settings.GetOrNullAsync(name);
        if (string.IsNullOrWhiteSpace(raw))
        {
            raw = SharedWorkspaceRoutePriceSettings.DefaultValue(name);
        }

        // An explicit 0 disables the floor. It does not fall back to the definition default.
        if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) || value <= 0)
        {
            return null;
        }

        return value;
    }
}

public class SharedWorkspaceRoutePriceSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        foreach (var (name, value) in SharedWorkspaceRoutePriceSettings.All)
        {
            var setting = new SettingDefinition(name, value, isVisibleToClients: false);
            setting.Providers.Add(DefaultValueSettingValueProvider.ProviderName);
            setting.Providers.Add(ConfigurationSettingValueProvider.ProviderName);
            setting.Providers.Add(GlobalSettingValueProvider.ProviderName);
            context.Add(setting);
        }
    }
}
