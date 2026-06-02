using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Features;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.SettingManagement;
using Volo.Abp;
using Volo.Abp.Settings;

namespace SufiChain.Chat.Connectors.Email.Settings;

[Authorize(ChatPermissions.Settings.Manage)]
public class ChatEmailConnectorSettingsAppService : ChatAppService, IChatEmailConnectorSettingsAppService
{
    protected ISettingManager SettingManager { get; }
    protected IChatEmailConnectorSettingsEncryption SettingsEncryption { get; }

    public ChatEmailConnectorSettingsAppService(
        ISettingManager settingManager,
        IChatEmailConnectorSettingsEncryption settingsEncryption)
    {
        SettingManager = settingManager;
        SettingsEncryption = settingsEncryption;
    }

    public virtual async Task<ChatEmailConnectorSettingsDto> GetAsync()
    {
        await CheckFeatureAsync();

        var inboundPassword = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.InboundPassword);
        var smtpPassword = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.SmtpPassword);

        var dto = new ChatEmailConnectorSettingsDto
        {
            Enabled = await SettingProvider.IsTrueAsync(ChatSettingNames.EmailConnector.Enabled),
            DefaultFromAddress = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.DefaultFromAddress),
            ReplyToAddress = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.ReplyToAddress),
            InboundHost = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.InboundHost),
            InboundPort = await GetIntAsync(ChatSettingNames.EmailConnector.InboundPort),
            InboundUseSsl = await SettingProvider.IsTrueAsync(ChatSettingNames.EmailConnector.InboundUseSsl),
            InboundUserName = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.InboundUserName),
            HasInboundPassword = !inboundPassword.IsNullOrWhiteSpace(),
            SmtpHost = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.SmtpHost),
            SmtpPort = await GetIntAsync(ChatSettingNames.EmailConnector.SmtpPort),
            SmtpUseSsl = await SettingProvider.IsTrueAsync(ChatSettingNames.EmailConnector.SmtpUseSsl),
            SmtpUserName = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.SmtpUserName),
            HasSmtpPassword = !smtpPassword.IsNullOrWhiteSpace()
        };

        if (Enum.TryParse<ChatInboundEmailProtocol>(
                await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.InboundProtocol),
                out var protocol))
        {
            dto.InboundProtocol = protocol;
        }

        return dto;
    }

    public virtual async Task UpdateAsync(UpdateChatEmailConnectorSettingsInput input)
    {
        await CheckFeatureAsync();

        await SetBoolAsync(ChatSettingNames.EmailConnector.Enabled, input.Enabled);
        await SetAsync(ChatSettingNames.EmailConnector.DefaultFromAddress, input.DefaultFromAddress);
        await SetAsync(ChatSettingNames.EmailConnector.ReplyToAddress, input.ReplyToAddress);
        await SetAsync(ChatSettingNames.EmailConnector.InboundProtocol, input.InboundProtocol.ToString());
        await SetAsync(ChatSettingNames.EmailConnector.InboundHost, input.InboundHost);
        await SetIntAsync(ChatSettingNames.EmailConnector.InboundPort, input.InboundPort);
        await SetBoolAsync(ChatSettingNames.EmailConnector.InboundUseSsl, input.InboundUseSsl);
        await SetAsync(ChatSettingNames.EmailConnector.InboundUserName, input.InboundUserName);

        if (!input.InboundPassword.IsNullOrWhiteSpace())
        {
            await SetAsync(
                ChatSettingNames.EmailConnector.InboundPassword,
                SettingsEncryption.Encrypt(input.InboundPassword));
        }

        await SetAsync(ChatSettingNames.EmailConnector.SmtpHost, input.SmtpHost);
        await SetIntAsync(ChatSettingNames.EmailConnector.SmtpPort, input.SmtpPort);
        await SetBoolAsync(ChatSettingNames.EmailConnector.SmtpUseSsl, input.SmtpUseSsl);
        await SetAsync(ChatSettingNames.EmailConnector.SmtpUserName, input.SmtpUserName);

        if (!input.SmtpPassword.IsNullOrWhiteSpace())
        {
            await SetAsync(
                ChatSettingNames.EmailConnector.SmtpPassword,
                SettingsEncryption.Encrypt(input.SmtpPassword));
        }
    }

    protected virtual async Task CheckFeatureAsync()
    {
        if (!await FeatureChecker.IsEnabledAsync(ChatFeatures.EmailConnector))
        {
            throw new BusinessException(ChatErrorCodes.EmailConnectorDisabled);
        }
    }

    protected virtual async Task<int> GetIntAsync(string name)
    {
        return int.TryParse(await SettingProvider.GetOrNullAsync(name), out var value) ? value : 0;
    }

    protected virtual Task SetIntAsync(string name, int value)
    {
        return SetAsync(name, value.ToString());
    }

    protected virtual Task SetBoolAsync(string name, bool value)
    {
        return SetAsync(name, value.ToString().ToLowerInvariant());
    }

    protected virtual Task SetAsync(string name, string? value)
    {
        return SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, name, value);
    }
}
