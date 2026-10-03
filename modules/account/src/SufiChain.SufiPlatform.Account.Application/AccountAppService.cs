using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Account.Otp;
using SufiChain.SufiPlatform.Captcha;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Account;

public class AccountAppService : SufiApplicationService, IAccountAppService
{
    protected IdentityUserManager UserManager { get; }
    protected IIdentityUserRepository UserRepository { get; }
    protected IOptions<IdentityOptions> IdentityOptions { get; }
    protected IdentityUserToIdentityUserDtoMapper UserMapper { get; }
    protected ILocalEventBus LocalEventBus { get; }
    protected ICaptchaValidator CaptchaValidator { get; }
    protected IOtpCodeStore OtpCodeStore { get; }
    protected OtpResendCooldownGuard ResendCooldownGuard { get; }
    new ISettingProvider SettingProvider { get; }

    public AccountAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IOptions<IdentityOptions> identityOptions,
        IdentityUserToIdentityUserDtoMapper userMapper,
        ILocalEventBus localEventBus,
        ICaptchaValidator captchaValidator,
        IOtpCodeStore otpCodeStore,
        ISettingProvider settingProvider,
        OtpResendCooldownGuard resendCooldownGuard)
    {
        UserManager = userManager;
        UserRepository = userRepository;
        IdentityOptions = identityOptions;
        UserMapper = userMapper;
        LocalEventBus = localEventBus;
        CaptchaValidator = captchaValidator;
        OtpCodeStore = otpCodeStore;
        SettingProvider = settingProvider;
        ResendCooldownGuard = resendCooldownGuard;
    }

    public virtual async Task<IdentityUserDto> RegisterAsync(RegisterDto input)
    {
        await ValidateCaptchaAsync(input, CaptchaPurpose.Register);

        if (!await SettingProvider.IsTrueAsync(IdentitySettingNames.Registration.EnableSelfRegistration))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.SelfRegistrationDisabled);
        }

        var requirePhoneConfirmation = await IdentityPhoneConfirmationRules.IsRequiredForRegistrationAsync(SettingProvider);
        if (requirePhoneConfirmation && string.IsNullOrWhiteSpace(input.PhoneNumber))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.PhoneNumberRequired);
        }

        var user = new IdentityUser(
            GuidGenerator.Create(),
            input.UserName,
            input.EmailAddress,
            CurrentTenant.Id
        );

        (await UserManager.CreateAsync(user, input.Password)).CheckErrors();

        await UserManager.SetEmailAsync(user, input.EmailAddress);
        await UserManager.AddDefaultRolesAsync(user);

        if (requirePhoneConfirmation)
        {
            var phone = VerificationIdentifierHelper.NormalizePhone(input.PhoneNumber!);
            user.SetPhoneNumber(phone, confirmed: false);
            (await UserManager.UpdateAsync(user)).CheckErrors();
            await SendRegistrationPhoneConfirmationCodeAsync(user, phone);
        }

        string? confirmationToken = null;
        if (await IdentityEmailConfirmationRules.ShouldSendRegistrationConfirmationEmailAsync(SettingProvider))
        {
            confirmationToken = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        }

        await LocalEventBus.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = input.EmailAddress,
            AppName = input.AppName,
            EmailConfirmationToken = confirmationToken,
            ReturnUrl = input.ReturnUrl,
            ReturnUrlHash = input.ReturnUrlHash
        });

        return UserMapper.Map(user);
    }

    public virtual async Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input)
    {
        await ValidateCaptchaAsync(input, CaptchaPurpose.ForgotPassword);

        var user = await UserManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            throw new BusinessException(AccountErrorCodes.UserNotFoundWithEmail);
        }

        var resetToken = await UserManager.GeneratePasswordResetTokenAsync(user);

        await LocalEventBus.PublishAsync(new PasswordResetRequestedEvent
        {
            UserId = user.Id,
            Email = input.Email,
            ResetToken = resetToken,
            AppName = input.AppName,
            ReturnUrl = input.ReturnUrl,
            ReturnUrlHash = input.ReturnUrlHash
        });
    }

    public virtual async Task<bool> VerifyPasswordResetTokenAsync(VerifyPasswordResetTokenInput input)
    {
        var user = await UserRepository.FindAsync(input.UserId);
        if (user == null)
        {
            return false;
        }

        return await UserManager.VerifyUserTokenAsync(
            user,
            UserManager.Options.Tokens.PasswordResetTokenProvider,
            "ResetPassword",
            input.ResetToken
        );
    }

    public virtual async Task ResetPasswordAsync(ResetPasswordDto input)
    {
        var user = await UserRepository.GetAsync(input.UserId);

        (await UserManager.ResetPasswordAsync(user, input.ResetToken, input.Password))
            .CheckErrors();
    }

    public virtual async Task SendEmailConfirmationTokenAsync(SendEmailConfirmationTokenDto input)
    {
        await ValidateCaptchaAsync(input, CaptchaPurpose.EmailConfirmationResend);

        var user = await UserManager.FindByEmailAsync(input.Email);
        if (user == null || user.EmailConfirmed)
        {
            return;
        }

        var confirmationToken = await UserManager.GenerateEmailConfirmationTokenAsync(user);

        await LocalEventBus.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = input.Email,
            AppName = input.AppName ?? string.Empty,
            EmailConfirmationToken = confirmationToken,
            ReturnUrl = input.ReturnUrl,
            ReturnUrlHash = input.ReturnUrlHash
        });
    }

    public virtual async Task ConfirmEmailAsync(ConfirmEmailDto input)
    {
        var user = await UserRepository.GetAsync(input.UserId);

        (await UserManager.ConfirmEmailAsync(user, input.ConfirmationToken)).CheckErrors();
    }

    public virtual async Task<PhoneConfirmationStateDto> GetPhoneConfirmationStateAsync(Guid userId)
    {
        var user = await UserRepository.GetAsync(userId);
        return new PhoneConfirmationStateDto
        {
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            EmailConfirmed = user.EmailConfirmed,
            EmailConfirmationRequired = await IdentityEmailConfirmationRules.IsSignInBlockedUntilEmailConfirmedAsync(SettingProvider)
                && !user.EmailConfirmed,
            Email = user.Email
        };
    }

    public virtual async Task<OtpSendResultDto> SendPhoneConfirmationCodeAsync(SendPhoneConfirmationCodeDto input)
    {
        var user = await UserRepository.GetAsync(input.UserId);
        if (!await IdentityPhoneConfirmationRules.IsSmsProviderConfiguredAsync(SettingProvider))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.VerificationChannelUnavailable);
        }

        var phone = string.IsNullOrWhiteSpace(input.PhoneNumber)
            ? user.PhoneNumber
            : VerificationIdentifierHelper.NormalizePhone(input.PhoneNumber);
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.PhoneNumberRequired);
        }

        var resendAfterSeconds = await ResendCooldownGuard.EnsureAllowedAsync(VerificationDeliveryChannel.Sms, phone);

        if (!string.Equals(user.PhoneNumber, phone, StringComparison.Ordinal) || user.PhoneNumberConfirmed)
        {
            user.SetPhoneNumber(phone, confirmed: false);
            (await UserManager.UpdateAsync(user)).CheckErrors();
        }

        await SendPhoneConfirmationCodeAsync(user, phone);

        return new OtpSendResultDto { ResendAfterSeconds = resendAfterSeconds };
    }

    public virtual async Task<ConfirmPhoneNumberResultDto> ConfirmPhoneNumberAsync(ConfirmPhoneNumberDto input)
    {
        var user = await UserRepository.GetAsync(input.UserId);
        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.PhoneNumberRequired);
        }

        if (!user.PhoneNumberConfirmed)
        {
            var phone = VerificationIdentifierHelper.NormalizePhone(user.PhoneNumber);
            if (!await VerifyPhoneConfirmationCodeAsync(phone, input.Code))
            {
                throw new BusinessException(IdentitySecurityErrorCodes.OtpInvalidOrExpired);
            }

            user.SetPhoneNumber(phone, confirmed: true);
            (await UserManager.UpdateAsync(user)).CheckErrors();
        }

        var emailConfirmationRequired = await IdentityEmailConfirmationRules.IsSignInBlockedUntilEmailConfirmedAsync(SettingProvider)
            && !user.EmailConfirmed;
        return new ConfirmPhoneNumberResultDto
        {
            EmailConfirmationStillRequired = emailConfirmationRequired,
            Email = user.Email
        };
    }

    public virtual async Task<bool> VerifyEmailConfirmationTokenAsync(VerifyEmailConfirmationTokenInput input)
    {
        var user = await UserRepository.FindAsync(input.UserId);
        if (user == null)
        {
            return false;
        }

        return await UserManager.VerifyUserTokenAsync(
            user,
            UserManager.Options.Tokens.EmailConfirmationTokenProvider,
            "EmailConfirmation",
            input.ConfirmationToken
        );
    }

    /// <summary>
    /// A cooldown hit must not fail the registration itself; the user can resend from the phone confirmation page.
    /// </summary>
    protected virtual async Task SendRegistrationPhoneConfirmationCodeAsync(IdentityUser user, string phone)
    {
        try
        {
            await ResendCooldownGuard.EnsureAllowedAsync(VerificationDeliveryChannel.Sms, phone);
        }
        catch (BusinessException ex) when (ex.Code == IdentitySecurityErrorCodes.OtpResendCooldown)
        {
            return;
        }

        await SendPhoneConfirmationCodeAsync(user, phone);
    }

    protected virtual async Task SendPhoneConfirmationCodeAsync(IdentityUser user, string phone)
    {
        var maxPerHour = await SettingProvider.GetAsync<int>(IdentitySettingNames.Otp.RateLimitPerIdentifierPerHour);
        var allowed = await OtpCodeStore.TryIncrementRateLimitAsync(
            VerificationPurpose.PhoneConfirmation,
            VerificationDeliveryChannel.Sms,
            phone,
            maxPerHour);
        if (!allowed)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.OtpRateLimitExceeded);
        }

        var length = await SettingProvider.GetAsync<int>(IdentitySettingNames.Tokens.OtpLength);
        var code = OtpCodeGenerator.Generate(length);
        var expirationMinutes = await SettingProvider.GetAsync<int>(IdentitySettingNames.Tokens.OtpTokenLifespanMinutes);
        await OtpCodeStore.StoreAsync(
            VerificationPurpose.PhoneConfirmation,
            VerificationDeliveryChannel.Sms,
            phone,
            new OtpCacheItem
            {
                CodeHash = OtpCodeHasher.Hash(code),
                Attempts = 0,
                UserId = user.Id
            },
            expirationMinutes);

        await LocalEventBus.PublishAsync(new VerificationCodeRequestedEvent
        {
            UserId = user.Id,
            Identifier = phone,
            Code = code,
            Purpose = VerificationPurpose.PhoneConfirmation,
            PreferredChannel = VerificationDeliveryChannel.Sms,
            AppName = "Account"
        });
    }

    protected virtual async Task<bool> VerifyPhoneConfirmationCodeAsync(string phone, string code)
    {
        var cacheItem = await OtpCodeStore.GetAsync(
            VerificationPurpose.PhoneConfirmation,
            VerificationDeliveryChannel.Sms,
            phone);
        if (cacheItem == null || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var maxAttempts = await SettingProvider.GetAsync<int>(IdentitySettingNames.Otp.MaxAttemptsPerCode);
        if (maxAttempts > 0 && cacheItem.Attempts >= maxAttempts)
        {
            await OtpCodeStore.RemoveAsync(VerificationPurpose.PhoneConfirmation, VerificationDeliveryChannel.Sms, phone);
            return false;
        }

        if (!string.Equals(cacheItem.CodeHash, OtpCodeHasher.Hash(code), StringComparison.OrdinalIgnoreCase))
        {
            cacheItem.Attempts++;
            var expirationMinutes = await SettingProvider.GetAsync<int>(IdentitySettingNames.Tokens.OtpTokenLifespanMinutes);
            await OtpCodeStore.StoreAsync(
                VerificationPurpose.PhoneConfirmation,
                VerificationDeliveryChannel.Sms,
                phone,
                cacheItem,
                expirationMinutes);
            return false;
        }

        await OtpCodeStore.RemoveAsync(VerificationPurpose.PhoneConfirmation, VerificationDeliveryChannel.Sms, phone);
        return true;
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
}
