using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.SufiAI.Localization;
using SufiChain.SufiPlatform.SufiAI.MCP;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.SufiAI.MCP.IranMcp;

public sealed class IranMcpTokenVerifyOptions
{
    public IranMcpTokenVerifyMode Mode { get; init; } = IranMcpTokenVerifyMode.Off;

    public string? VerifyUrl { get; init; }
}

public interface IIranMcpTokenVerifyOptionsReader
{
    Task<IranMcpTokenVerifyOptions> GetAsync(CancellationToken cancellationToken = default);
}

public class IranMcpSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        Add(context, IranMcpSettings.TokenVerifyMode, IranMcpSettings.DefaultTokenVerifyMode);
        Add(context, IranMcpSettings.TokenVerifyUrl, string.Empty);
        Add(context, IranMcpSettings.Environment, string.Empty);
        Add(context, IranMcpSettings.AllowUpstreamExecution, IranMcpSettings.DefaultAllowUpstreamExecution);
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
/// Reads iran-mcp token-verify settings. Required mode with an empty URL fails closed
/// with a localized error instead of silently skipping verification.
/// </summary>
public class IranMcpTokenVerifyOptionsReader : IIranMcpTokenVerifyOptionsReader, ITransientDependency
{
    private readonly ISettingProvider _settings;
    private readonly IStringLocalizer<AIResource> _localizer;

    public IranMcpTokenVerifyOptionsReader(
        ISettingProvider settings,
        IStringLocalizer<AIResource> localizer)
    {
        _settings = settings;
        _localizer = localizer;
    }

    public virtual async Task<IranMcpTokenVerifyOptions> GetAsync(CancellationToken cancellationToken = default)
    {
        var modeText = await _settings.GetOrNullAsync(IranMcpSettings.TokenVerifyMode);
        var mode = ParseMode(modeText);
        var url = (await _settings.GetOrNullAsync(IranMcpSettings.TokenVerifyUrl))?.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            url = null;
        }

        if (mode == IranMcpTokenVerifyMode.Required && url is null)
        {
            throw new BusinessException(AIErrorCodes.IranMcpTokenVerifyUrlRequired)
                .WithData("Message", _localizer[AIErrorCodes.IranMcpTokenVerifyUrlRequired].Value);
        }

        return new IranMcpTokenVerifyOptions
        {
            Mode = mode,
            VerifyUrl = url
        };
    }

    public static IranMcpTokenVerifyMode ParseMode(string? value)
    {
        var text = (value ?? IranMcpSettings.DefaultTokenVerifyMode).Trim().ToLowerInvariant();
        return text switch
        {
            "optional" => IranMcpTokenVerifyMode.Optional,
            "required" => IranMcpTokenVerifyMode.Required,
            "off" or "disabled" or "none" or "deny" => IranMcpTokenVerifyMode.Off,
            _ => IranMcpTokenVerifyMode.Off
        };
    }
}
