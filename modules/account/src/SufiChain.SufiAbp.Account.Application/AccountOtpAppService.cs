using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SufiChain.SufiAbp.Account.Otp;
using SufiChain.SufiAbp.Captcha;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.Settings;
using SufiChain.SufiAbp.UI.Abstractions.Account;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Account;

public class AccountOtpAppService : SufiAbpApplicationService, IAccountOtpAppService
{
    protected IdentityUserManager UserManager { get; }

    protected IIdentityUserRepository UserRepository { get; }

    protected IdentityUserToIdentityUserDtoMapper UserMapper { get; }

    protected ISettingProvider SettingProvider { get; }

    protected ICaptchaValidator CaptchaValidator { get; }

    protected IVerificationChannelResolver ChannelResolver { get; }

    protected IVerificationChannelAvailabilityChecker ChannelAvailabilityChecker { get; }

    protected IOtpCodeStore OtpCodeStore { get; }

    protected ILocalEventBus LocalEventBus { get; }

    protected ILoginCompletionTokenStore LoginCompletionTokenStore { get; }

    public AccountOtpAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IdentityUserToIdentityUserDtoMapper userMapper,
        ISettingProvider settingProvider,
        ICaptchaValidator captchaValidator,
        IVerificationChannelResolver channelResolver,
        IVerificationChannelAvailabilityChecker channelAvailabilityChecker,
        IOtpCodeStore otpCodeStore,
        ILocalEventBus localEventBus,
        ILoginCompletionTokenStore loginCompletionTokenStore)
    {
        UserManager = userManager;
        UserRepository = userRepository;
        UserMapper = userMapper;
        SettingProvider = settingProvider;
        CaptchaValidator = captchaValidator;
        ChannelResolver = channelResolver;
        ChannelAvailabilityChecker = channelAvailabilityChecker;
        OtpCodeStore = otpCodeStore;
        LocalEventBus = localEventBus;
        LoginCompletionTokenStore = loginCompletionTokenStore;
    }

    [AllowAnonymous]
    public virtual async Task<OtpOptionsDto> GetOtpOptionsAsync()
    {
        var isEnabled = await SettingProvider.IsTrueAsync(IdentitySettingNames.Otp.IsEnabled);
        var available = await GetAllowedOtpChannelsAsync();

        var defaultChannel = await ChannelResolver.ResolveAsync(
            VerificationPurpose.OtpLogin,
            available.FirstOrDefault());

        return new OtpOptionsDto
        {
            IsEnabled = isEnabled,
            AllowLogin = isEnabled && await SettingProvider.IsTrueAsync(IdentitySettingNames.Otp.AllowLogin),
            AllowRegistration = isEnabled &&
                                await SettingProvider.IsTrueAsync(IdentitySettingNames.Otp.AllowRegistration),
            DefaultChannel = defaultChannel,
            AvailableChannels = available
        };
    }

    [AllowAnonymous]
    public virtual async Task SendLoginOtpAsync(SendOtpInput input)
    {
        await EnsureOtpLoginEnabledAsync();
        await ValidateCaptchaAsync(input, CaptchaPurpose.OtpSend);

        var channel = await ChannelResolver.ResolveAsync(VerificationPurpose.OtpLogin, input.Channel);
        var identifier = VerificationIdentifierHelper.NormalizeIdentifier(channel, input.Identifier);

        await EnsureRateLimitAsync(VerificationPurpose.OtpLogin, channel, identifier);

        var user = await VerificationIdentifierHelper.FindUserByIdentifierAsync(
            UserManager, UserRepository, channel, input.Identifier);
        if (user == null)
        {
            return;
        }

        if (channel.IsPhoneChannel())
        {
            await VerificationIdentifierHelper.EnsurePhoneReadyForDeliveryAsync(
                user, UserManager, SettingProvider, channel);
        }

        var code = await GenerateAndStoreOtpAsync(VerificationPurpose.OtpLogin, channel, identifier, user.Id);

        await LocalEventBus.PublishAsync(new VerificationCodeRequestedEvent
        {
            UserId = user.Id,
            Identifier = identifier,
            Code = code,
            Purpose = VerificationPurpose.OtpLogin,
            PreferredChannel = channel,
            AppName = input.AppName
        });
    }

    [AllowAnonymous]
    public virtual async Task<VerifyLoginOtpResultDto> VerifyLoginOtpAsync(VerifyLoginOtpInput input)
    {
        await EnsureOtpLoginEnabledAsync();

        if (!LoginCompletionTokenStore.IsSupported)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.AuthenticationNotAvailable);
        }

        var channel = await ChannelResolver.ResolveAsync(VerificationPurpose.OtpLogin, input.Channel);
        var identifier = VerificationIdentifierHelper.NormalizeIdentifier(channel, input.Identifier);

        var user = await VerificationIdentifierHelper.FindUserByIdentifierAsync(
            UserManager, UserRepository, channel, input.Identifier);
        if (user == null || !await VerifyOtpCodeAsync(VerificationPurpose.OtpLogin, channel, identifier, input.Code))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.OtpInvalidOrExpired);
        }

        if (!await UserManager.IsEmailConfirmedAsync(user) &&
            await SettingProvider.IsTrueAsync(IdentitySettingNames.SignIn.RequireConfirmedEmail))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.EmailConfirmationRequired);
        }

        var loginToken = await LoginCompletionTokenStore.CreateAsync(
            user.Id,
            input.ReturnUrl,
            input.RememberMe);

        return new VerifyLoginOtpResultDto
        {
            LoginCompletionToken = loginToken,
            ReturnUrl = input.ReturnUrl
        };
    }

    [AllowAnonymous]
    public virtual async Task SendRegistrationOtpAsync(SendOtpInput input)
    {
        await EnsureOtpRegistrationEnabledAsync();
        await ValidateCaptchaAsync(input, CaptchaPurpose.OtpSend);

        var channel = await ChannelResolver.ResolveAsync(VerificationPurpose.OtpRegistration, input.Channel);
        var identifier = VerificationIdentifierHelper.NormalizeIdentifier(channel, input.Identifier);

        await EnsureRateLimitAsync(VerificationPurpose.OtpRegistration, channel, identifier);

        var existingUser = channel == VerificationDeliveryChannel.Email
            ? await UserManager.FindByEmailAsync(identifier)
            : await VerificationIdentifierHelper.FindByPhoneNumberAsync(UserRepository, identifier);
        if (existingUser != null)
        {
            return;
        }

        var code = await GenerateAndStoreOtpAsync(VerificationPurpose.OtpRegistration, channel, identifier, null);

        await LocalEventBus.PublishAsync(new VerificationCodeRequestedEvent
        {
            Identifier = identifier,
            Code = code,
            Purpose = VerificationPurpose.OtpRegistration,
            PreferredChannel = channel,
            AppName = input.AppName
        });
    }

    [AllowAnonymous]
    public virtual async Task<VerifyRegistrationOtpResultDto> VerifyRegistrationOtpAsync(VerifyOtpInput input)
    {
        await EnsureOtpRegistrationEnabledAsync();

        var channel = await ChannelResolver.ResolveAsync(VerificationPurpose.OtpRegistration, input.Channel);
        var identifier = VerificationIdentifierHelper.NormalizeIdentifier(channel, input.Identifier);

        if (!await VerifyOtpCodeAsync(VerificationPurpose.OtpRegistration, channel, identifier, input.Code))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.OtpInvalidOrExpired);
        }

        var expirationMinutes = await SettingProvider.GetAsync<int>(
            IdentitySettingNames.Tokens.OtpTokenLifespanMinutes);

        var registrationToken = await OtpCodeStore.CreateRegistrationTokenAsync(
            identifier,
            expirationMinutes);

        return new VerifyRegistrationOtpResultDto
        {
            RegistrationToken = registrationToken
        };
    }

    [AllowAnonymous]
    public virtual async Task<IdentityUserDto> RegisterWithOtpAsync(RegisterWithOtpDto input)
    {
        await EnsureOtpRegistrationEnabledAsync();

        if (!await SettingProvider.IsTrueAsync(IdentitySettingNames.Registration.EnableSelfRegistration))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.SelfRegistrationDisabled);
        }

        var verifiedIdentifier = await OtpCodeStore.ConsumeRegistrationTokenAsync(input.RegistrationToken);
        if (verifiedIdentifier == null)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.OtpInvalidOrExpired);
        }

        var email = VerificationIdentifierHelper.NormalizeEmail(input.EmailAddress);
        var verifiedIsEmail = verifiedIdentifier.Contains('@', StringComparison.Ordinal);

        if (verifiedIsEmail)
        {
            if (!string.Equals(verifiedIdentifier, email, StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException(IdentitySecurityErrorCodes.OtpInvalidOrExpired);
            }
        }
        else if (string.IsNullOrWhiteSpace(email))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.OtpInvalidOrExpired);
        }

        var user = new IdentityUser(
            GuidGenerator.Create(),
            input.UserName,
            email,
            CurrentTenant.Id);

        (await UserManager.CreateAsync(user, input.Password)).CheckErrors();
        await UserManager.SetEmailAsync(user, email);

        if (!verifiedIsEmail)
        {
            (await UserManager.SetPhoneNumberAsync(user, verifiedIdentifier)).CheckErrors();
            user.SetPhoneNumber(verifiedIdentifier, confirmed: true);
            (await UserManager.UpdateAsync(user)).CheckErrors();
        }

        await UserManager.AddDefaultRolesAsync(user);

        var emailConfirmToken = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        (await UserManager.ConfirmEmailAsync(user, emailConfirmToken)).CheckErrors();

        await LocalEventBus.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = email,
            AppName = input.AppName,
            EmailConfirmationToken = null,
            ReturnUrl = input.ReturnUrl,
            ReturnUrlHash = input.ReturnUrlHash
        });

        return UserMapper.Map(user);
    }

    protected virtual async Task EnsureOtpLoginEnabledAsync()
    {
        if (!await SettingProvider.IsTrueAsync(IdentitySettingNames.Otp.IsEnabled) ||
            !await SettingProvider.IsTrueAsync(IdentitySettingNames.Otp.AllowLogin))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.OtpDisabled);
        }
    }

    protected virtual async Task EnsureOtpRegistrationEnabledAsync()
    {
        if (!await SettingProvider.IsTrueAsync(IdentitySettingNames.Otp.IsEnabled) ||
            !await SettingProvider.IsTrueAsync(IdentitySettingNames.Otp.AllowRegistration))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.OtpDisabled);
        }
    }

    protected virtual async Task ValidateCaptchaAsync(CaptchaInputDto input, CaptchaPurpose purpose)
    {
        var result = await CaptchaValidator.ValidateAsync(new CaptchaValidationContext
        {
            Purpose = purpose,
            ChallengeId = input.CaptchaChallengeId,
            Answer = input.CaptchaAnswer,
            Token = input.CaptchaToken
        });

        if (!result.IsValid)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.CaptchaValidationFailed);
        }
    }

    protected virtual async Task EnsureRateLimitAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier)
    {
        var maxPerHour = await SettingProvider.GetAsync<int>(
            IdentitySettingNames.Otp.RateLimitPerIdentifierPerHour);

        var allowed = await OtpCodeStore.TryIncrementRateLimitAsync(
            purpose,
            channel,
            identifier,
            maxPerHour);

        if (!allowed)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.OtpRateLimitExceeded);
        }
    }

    protected virtual async Task<string> GenerateAndStoreOtpAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier,
        Guid? userId)
    {
        var length = await SettingProvider.GetAsync<int>(IdentitySettingNames.Tokens.OtpLength);
        if (length < 4)
        {
            length = 6;
        }

        var code = GenerateNumericCode(length);
        var expirationMinutes = await SettingProvider.GetAsync<int>(
            IdentitySettingNames.Tokens.OtpTokenLifespanMinutes);

        await OtpCodeStore.StoreAsync(
            purpose,
            channel,
            identifier,
            new OtpCacheItem
            {
                CodeHash = OtpCodeHasher.Hash(code),
                Attempts = 0,
                UserId = userId
            },
            expirationMinutes);

        return code;
    }

    protected virtual async Task<bool> VerifyOtpCodeAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier,
        string code)
    {
        var cacheItem = await OtpCodeStore.GetAsync(purpose, channel, identifier);
        if (cacheItem == null)
        {
            return false;
        }

        var maxAttempts = await SettingProvider.GetAsync<int>(IdentitySettingNames.Otp.MaxAttemptsPerCode);
        if (maxAttempts > 0 && cacheItem.Attempts >= maxAttempts)
        {
            await OtpCodeStore.RemoveAsync(purpose, channel, identifier);
            return false;
        }

        var isValid = string.Equals(cacheItem.CodeHash, OtpCodeHasher.Hash(code), StringComparison.OrdinalIgnoreCase);
        if (!isValid)
        {
            cacheItem.Attempts++;
            var expirationMinutes = await SettingProvider.GetAsync<int>(
                IdentitySettingNames.Tokens.OtpTokenLifespanMinutes);
            await OtpCodeStore.StoreAsync(purpose, channel, identifier, cacheItem, expirationMinutes);
            return false;
        }

        await OtpCodeStore.RemoveAsync(purpose, channel, identifier);
        return true;
    }

    protected virtual async Task<IReadOnlyList<VerificationDeliveryChannel>> GetAllowedOtpChannelsAsync()
    {
        var allowedSetting = await SettingProvider.GetOrNullAsync(IdentitySettingNames.Otp.AllowedChannels);
        var allowedNames = ParseChannelList(allowedSetting);
        var available = await ChannelAvailabilityChecker.GetAvailableChannelsAsync();

        return available
            .Where(c => allowedNames.Contains(c.ToString(), StringComparer.OrdinalIgnoreCase))
            .ToList();
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

    protected static string GenerateNumericCode(int length)
    {
        var max = (int)Math.Pow(10, length);
        var value = Random.Shared.Next(0, max);
        return value.ToString($"D{length}");
    }

}
