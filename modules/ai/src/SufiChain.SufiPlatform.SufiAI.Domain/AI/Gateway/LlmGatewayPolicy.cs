using System.Globalization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// One resolved gateway policy. Attempts stay inside 3 to 5. The SDK client keeps maxRetries 0.
/// </summary>
public sealed class LlmGatewayPolicy
{
    public int MaxAttempts { get; init; } = 4;

    public int BaseDelayMs { get; init; } = 200;

    public int MaxDelayMs { get; init; } = 8_000;

    public double RetryBudgetFraction { get; init; } = 0.10;

    public int BreakerMinimumSamples { get; init; } = 4;

    public int BreakerWindow { get; init; } = 8;

    public double BreakerErrorRate { get; init; } = 0.50;

    public TimeSpan BreakerOpen { get; init; } = TimeSpan.FromSeconds(30);

    public int MaxToolRounds { get; init; } = 10;

    public int MaxTurnSeconds { get; init; } = LlmGatewaySettings.DefaultMaxTurnSeconds;

    public int ClampAttempts() => Math.Clamp(MaxAttempts, 3, 5);

    public static LlmGatewayPolicy Default { get; } = new();
}

public interface ILlmGatewayPolicyReader
{
    Task<LlmGatewayPolicy> GetAsync(CancellationToken cancellationToken = default);
}

public class LlmGatewaySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        Add(context, LlmGatewaySettings.MaxAttempts, "4");
        Add(context, LlmGatewaySettings.BaseDelayMs, "200");
        Add(context, LlmGatewaySettings.MaxDelayMs, "8000");
        Add(context, LlmGatewaySettings.RetryBudgetPercent, "10");
        Add(context, LlmGatewaySettings.BreakerErrorRatePercent, "50");
        Add(context, LlmGatewaySettings.BreakerMinimumSamples, "4");
        Add(context, LlmGatewaySettings.BreakerWindow, "8");
        Add(context, LlmGatewaySettings.BreakerOpenSeconds, "30");
        Add(context, LlmGatewaySettings.MaxToolRounds, "10");
        Add(context, LlmGatewaySettings.MaxTurnSeconds, LlmGatewaySettings.DefaultMaxTurnSeconds.ToString(CultureInfo.InvariantCulture));
    }

    private static void Add(ISettingDefinitionContext context, string name, string value)
    {
        var setting = new SettingDefinition(name, value, isVisibleToClients: false);
        setting.Providers.Add(DefaultValueSettingValueProvider.ProviderName);
        setting.Providers.Add(ConfigurationSettingValueProvider.ProviderName);
        setting.Providers.Add(GlobalSettingValueProvider.ProviderName);
        context.Add(setting);
    }
}

/// <summary>
/// Reads the host <see cref="LlmGatewaySettings"/>. Tenant values are not stored.
/// </summary>
public class LlmGatewayPolicyReader : ILlmGatewayPolicyReader, ITransientDependency
{
    private readonly ISettingProvider _settings;

    public LlmGatewayPolicyReader(ISettingProvider settings)
    {
        _settings = settings;
    }

    public virtual async Task<LlmGatewayPolicy> GetAsync(CancellationToken cancellationToken = default)
    {
        return new LlmGatewayPolicy
        {
            MaxAttempts = await Number(LlmGatewaySettings.MaxAttempts, 4, 3, 5),
            BaseDelayMs = await Number(LlmGatewaySettings.BaseDelayMs, 200, 0, 60_000),
            MaxDelayMs = await Number(LlmGatewaySettings.MaxDelayMs, 8_000, 0, 120_000),
            RetryBudgetFraction = await Number(LlmGatewaySettings.RetryBudgetPercent, 10, 0, 100) / 100d,
            BreakerErrorRate = await Number(LlmGatewaySettings.BreakerErrorRatePercent, 50, 1, 100) / 100d,
            BreakerMinimumSamples = await Number(LlmGatewaySettings.BreakerMinimumSamples, 4, 1, 100),
            BreakerWindow = await Number(LlmGatewaySettings.BreakerWindow, 8, 1, 100),
            BreakerOpen = TimeSpan.FromSeconds(await Number(LlmGatewaySettings.BreakerOpenSeconds, 30, 1, 3_600)),
            MaxToolRounds = await Number(LlmGatewaySettings.MaxToolRounds, 10, 1, 50),
            MaxTurnSeconds = await Number(LlmGatewaySettings.MaxTurnSeconds, LlmGatewaySettings.DefaultMaxTurnSeconds, 0, 3_600)
        };
    }

    private async Task<int> Number(string name, int fallback, int min, int max)
    {
        var raw = await _settings.GetOrNullAsync(name);
        var parsed = int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
        return Math.Clamp(parsed, min, max);
    }
}
