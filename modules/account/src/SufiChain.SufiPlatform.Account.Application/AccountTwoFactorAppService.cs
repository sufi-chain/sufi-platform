using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Settings;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Settings;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Account;

public class AccountTwoFactorAppService : SufiApplicationService, IAccountTwoFactorAppService
{
    protected IdentityUserManager UserManager { get; }

    protected ISettingProvider SettingProvider { get; }

    protected ILocalEventBus LocalEventBus { get; }

    protected IVerificationChannelAvailabilityChecker ChannelAvailabilityChecker { get; }

    protected ITwoFactorPendingLoginStore PendingLoginStore { get; }

    protected ILoginCompletionTokenStore LoginCompletionTokenStore { get; }

    protected IAccountSecurityLogAppService SecurityLogAppService { get; }

    public AccountTwoFactorAppService(
        IdentityUserManager userManager,
        ISettingProvider settingProvider,
        ILocalEventBus localEventBus,
        IVerificationChannelAvailabilityChecker channelAvailabilityChecker,
        ITwoFactorPendingLoginStore pendingLoginStore,
        ILoginCompletionTokenStore loginCompletionTokenStore,
        IAccountSecurityLogAppService securityLogAppService)
    {
        UserManager = userManager;
        SettingProvider = settingProvider;
        LocalEventBus = localEventBus;
        ChannelAvailabilityChecker = channelAvailabilityChecker;
        PendingLoginStore = pendingLoginStore;
        LoginCompletionTokenStore = loginCompletionTokenStore;
        SecurityLogAppService = securityLogAppService;
    }

    [AllowAnonymous]
    public virtual async Task<TwoFactorLoginOptionsDto> GetLoginOptionsAsync()
    {
        return new TwoFactorLoginOptionsDto
        {
            AllowAuthenticatorApp = await SettingProvider.IsTrueAsync(
                IdentitySettingNames.TwoFactor.AllowAuthenticatorApp),
            AllowCodeDelivery = await SettingProvider.IsTrueAsync(
                IdentitySettingNames.TwoFactor.AllowCodeDelivery),
            AvailableCodeChannels = await GetAllowedCodeChannelsAsync()
        };
    }

    public virtual async Task<TwoFactorInfoDto> GetTwoFactorInfoAsync()
    {
        var user = await GetCurrentUserAsync();
        var hasAuthenticator = !string.IsNullOrEmpty(await UserManager.GetAuthenticatorKeyAsync(user));

        return new TwoFactorInfoDto
        {
            IsEnabled = user.TwoFactorEnabled,
            HasAuthenticator = hasAuthenticator,
            AllowAuthenticatorApp = await SettingProvider.IsTrueAsync(
                IdentitySettingNames.TwoFactor.AllowAuthenticatorApp),
            AllowCodeDelivery = await SettingProvider.IsTrueAsync(
                IdentitySettingNames.TwoFactor.AllowCodeDelivery),
            AvailableCodeChannels = await GetAllowedCodeChannelsAsync()
        };
    }

    public virtual async Task<AuthenticatorSetupDto> GenerateAuthenticatorSetupAsync()
    {
        await EnsureUsersCanChangeTwoFactorAsync();

        var user = await GetCurrentUserAsync();
        await UserManager.ResetAuthenticatorKeyAsync(user);

        var unformattedKey = await UserManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.TwoFactorSetupFailed);
        }

        var email = await UserManager.GetEmailAsync(user) ?? user.UserName ?? user.Id.ToString();
        var issuer = await GetAuthenticatorIssuerAsync();

        return new AuthenticatorSetupDto
        {
            SharedKey = FormatAuthenticatorKey(unformattedKey),
            AuthenticatorUri = BuildAuthenticatorUri(issuer, email, unformattedKey),
            Issuer = issuer
        };
    }

    public virtual async Task<RecoveryCodesDto> EnableTwoFactorAsync(EnableTwoFactorInput input)
    {
        await EnsureUsersCanChangeTwoFactorAsync();

        var user = await GetCurrentUserAsync();
        var isValid = await UserManager.VerifyTwoFactorTokenAsync(
            user,
            TwoFactorProviderNames.Authenticator,
            input.Code);

        if (!isValid)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.TwoFactorCodeInvalid);
        }

        await UserManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        await SecurityLogAppService.SaveLoginEventAsync(
            IdentitySecurityLogIdentityConsts.Identity,
            IdentitySecurityLogActionConsts.TwoFactorEnabled,
            user.UserName);

        return new RecoveryCodesDto
        {
            RecoveryCodes = recoveryCodes?.ToArray() ?? []
        };
    }

    public virtual async Task DisableTwoFactorAsync(DisableTwoFactorInput input)
    {
        await EnsureUsersCanChangeTwoFactorAsync();

        var user = await GetCurrentUserAsync();

        if (!await UserManager.CheckPasswordAsync(user, input.Password))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.InvalidPassword);
        }

        await UserManager.SetTwoFactorEnabledAsync(user, false);
        await UserManager.ResetAuthenticatorKeyAsync(user);

        await SecurityLogAppService.SaveLoginEventAsync(
            IdentitySecurityLogIdentityConsts.Identity,
            IdentitySecurityLogActionConsts.TwoFactorDisabled,
            user.UserName);
    }

    public virtual async Task<RecoveryCodesDto> GenerateRecoveryCodesAsync()
    {
        await EnsureUsersCanChangeTwoFactorAsync();

        var user = await GetCurrentUserAsync();
        if (!user.TwoFactorEnabled)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.TwoFactorNotEnabled);
        }

        var recoveryCodes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return new RecoveryCodesDto
        {
            RecoveryCodes = recoveryCodes?.ToArray() ?? []
        };
    }

    [AllowAnonymous]
    public virtual async Task SendTwoFactorCodeAsync(SendTwoFactorCodeInput input)
    {
        if (!await SettingProvider.IsTrueAsync(IdentitySettingNames.TwoFactor.AllowCodeDelivery))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.TwoFactorCodeDeliveryDisabled);
        }

        var user = await ResolveUserForTwoFactorAsync(input.PendingToken);
        var allowedChannels = await GetAllowedCodeChannelsAsync();
        if (allowedChannels.Count == 0)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.VerificationChannelUnavailable);
        }

        var preferred = input.PreferredChannel ?? VerificationDeliveryChannel.Email;

        if (!allowedChannels.Contains(preferred))
        {
            preferred = allowedChannels[0];
        }

        await VerificationIdentifierHelper.EnsurePhoneReadyForDeliveryAsync(
            user, UserManager, SettingProvider, preferred);

        var recipient = await VerificationIdentifierHelper.ResolveRecipientAsync(user, UserManager, preferred);
        var tokenProvider = VerificationIdentifierHelper.GetTwoFactorTokenProvider(preferred);
        var code = await UserManager.GenerateTwoFactorTokenAsync(user, tokenProvider);

        await LocalEventBus.PublishAsync(new VerificationCodeRequestedEvent
        {
            UserId = user.Id,
            Identifier = recipient,
            Code = code,
            Purpose = VerificationPurpose.TwoFactorCode,
            PreferredChannel = preferred,
            AppName = input.AppName
        });
    }

    [AllowAnonymous]
    public virtual async Task<CompleteTwoFactorLoginResultDto> CompleteTwoFactorLoginAsync(
        CompleteTwoFactorLoginInput input)
    {
        if (!PendingLoginStore.IsSupported || !LoginCompletionTokenStore.IsSupported)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.AuthenticationNotAvailable);
        }

        var pending = await PendingLoginStore.GetAsync(input.PendingToken);
        if (pending == null)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.TwoFactorPendingLoginExpired);
        }

        var (userId, returnUrl, rememberMe) = pending.Value;
        var user = await UserManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.TwoFactorPendingLoginExpired);
        }

        var verified = await VerifyTwoFactorLoginAsync(user, input);
        if (!verified)
        {
            await SecurityLogAppService.SaveLoginEventAsync(
                IdentitySecurityLogIdentityConsts.IdentityTwoFactor,
                IdentitySecurityLogActionConsts.LoginFailed,
                user.UserName);
            throw new BusinessException(IdentitySecurityErrorCodes.TwoFactorCodeInvalid);
        }

        await PendingLoginStore.ConsumeAsync(input.PendingToken);

        var loginToken = await LoginCompletionTokenStore.CreateAsync(userId, returnUrl, rememberMe);

        return new CompleteTwoFactorLoginResultDto
        {
            LoginCompletionToken = loginToken,
            ReturnUrl = returnUrl
        };
    }

    public virtual async Task<string?> GetPostLoginRedirectUrlAsync(Guid userId, string? returnUrl)
    {
        var user = await UserManager.FindByIdAsync(userId.ToString());
        if (user == null || user.TwoFactorEnabled)
        {
            return null;
        }

        var enforceAdmin = await SettingProvider.IsTrueAsync(
            IdentitySettingNames.TwoFactor.EnforceForAdministrators);
        var enforceNewUsers = await SettingProvider.IsTrueAsync(
            IdentitySettingNames.TwoFactor.EnforceForNewUsers);

        if (!enforceAdmin && !enforceNewUsers)
        {
            return null;
        }

        var roles = await UserManager.GetRolesAsync(user);
        var isAdmin = roles.Any(r => string.Equals(r, "admin", StringComparison.OrdinalIgnoreCase));

        if (enforceAdmin && isAdmin)
        {
            return BuildManageTwoFactorUrl(returnUrl);
        }

        if (enforceNewUsers && IsNewUser(user))
        {
            return BuildManageTwoFactorUrl(returnUrl);
        }

        return null;
    }

    protected virtual async Task<bool> VerifyTwoFactorLoginAsync(IdentityUser user, CompleteTwoFactorLoginInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.RecoveryCode))
        {
            var recoveryResult = await UserManager.RedeemTwoFactorRecoveryCodeAsync(user, input.RecoveryCode);
            return recoveryResult.Succeeded;
        }

        if (string.IsNullOrWhiteSpace(input.Code))
        {
            return false;
        }

        var provider = string.IsNullOrWhiteSpace(input.Provider)
            ? TwoFactorProviderNames.Authenticator
            : input.Provider;

        return await UserManager.VerifyTwoFactorTokenAsync(user, provider, input.Code);
    }

    protected virtual async Task EnsureUsersCanChangeTwoFactorAsync()
    {
        if (!await SettingProvider.IsTrueAsync(IdentitySettingNames.TwoFactor.UsersCanChange))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.TwoFactorChangeNotAllowed);
        }
    }

    protected virtual async Task<IdentityUser> GetCurrentUserAsync()
    {
        if (CurrentUser.Id == null)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.UserNotAuthenticated);
        }

        var user = await UserManager.FindByIdAsync(CurrentUser.Id.Value.ToString());
        if (user == null)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.UserNotFound);
        }

        return user;
    }

    protected virtual async Task<IdentityUser> ResolveUserForTwoFactorAsync(string? pendingToken)
    {
        if (!string.IsNullOrWhiteSpace(pendingToken))
        {
            var pending = await PendingLoginStore.GetAsync(pendingToken);
            if (pending == null)
            {
                throw new BusinessException(IdentitySecurityErrorCodes.TwoFactorPendingLoginExpired);
            }

            var user = await UserManager.FindByIdAsync(pending.Value.userId.ToString());
            if (user == null)
            {
                throw new BusinessException(IdentitySecurityErrorCodes.TwoFactorPendingLoginExpired);
            }

            return user;
        }

        return await GetCurrentUserAsync();
    }

    protected virtual async Task<IReadOnlyList<VerificationDeliveryChannel>> GetAllowedCodeChannelsAsync()
    {
        if (!await SettingProvider.IsTrueAsync(IdentitySettingNames.TwoFactor.AllowCodeDelivery))
        {
            return Array.Empty<VerificationDeliveryChannel>();
        }

        var allowedSetting = await SettingProvider.GetOrNullAsync(
            IdentitySettingNames.TwoFactor.AllowedCodeChannels);
        var allowedNames = ParseChannelList(allowedSetting);

        var available = await ChannelAvailabilityChecker.GetAvailableChannelsAsync();
        return available.Where(c => allowedNames.Contains(c.ToString(), StringComparer.OrdinalIgnoreCase)).ToList();
    }

    protected virtual HashSet<string> ParseChannelList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Email" };
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    protected virtual async Task<string> GetAuthenticatorIssuerAsync()
    {
        var appName = await SettingProvider.GetOrNullAsync("App:Name");
        return string.IsNullOrWhiteSpace(appName) ? "Sufi" : appName;
    }

    protected virtual string FormatAuthenticatorKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var index = 0;
        while (index < unformattedKey.Length)
        {
            var length = Math.Min(4, unformattedKey.Length - index);
            result.Append(unformattedKey.AsSpan(index, length));
            if (index + length < unformattedKey.Length)
            {
                result.Append(' ');
            }

            index += length;
        }

        return result.ToString();
    }

    protected virtual string BuildAuthenticatorUri(string issuer, string email, string unformattedKey)
    {
        return
            $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={unformattedKey}&issuer={Uri.EscapeDataString(issuer)}&digits=6";
    }

    protected virtual bool IsNewUser(IdentityUser user)
    {
        return user.CreationTime > Clock.Now.AddDays(-30);
    }

    protected virtual string BuildManageTwoFactorUrl(string? returnUrl)
    {
        const string baseUrl = "/panel/portal/profile?tab=two-factor";

        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return baseUrl;
        }

        return $"{baseUrl}&returnUrl={Uri.EscapeDataString(returnUrl)}";
    }
}
