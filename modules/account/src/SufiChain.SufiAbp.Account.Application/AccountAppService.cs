using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Captcha;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.Settings;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Account;

public class AccountAppService : SufiAbpApplicationService, IAccountAppService
{
    protected IdentityUserManager UserManager { get; }
    protected IIdentityUserRepository UserRepository { get; }
    protected IOptions<IdentityOptions> IdentityOptions { get; }
    protected IdentityUserToIdentityUserDtoMapper UserMapper { get; }
    protected ILocalEventBus LocalEventBus { get; }
    protected ICaptchaValidator CaptchaValidator { get; }
    protected ISettingProvider SettingProvider { get; }

    public AccountAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IOptions<IdentityOptions> identityOptions,
        IdentityUserToIdentityUserDtoMapper userMapper,
        ILocalEventBus localEventBus,
        ICaptchaValidator captchaValidator,
        ISettingProvider settingProvider)
    {
        UserManager = userManager;
        UserRepository = userRepository;
        IdentityOptions = identityOptions;
        UserMapper = userMapper;
        LocalEventBus = localEventBus;
        CaptchaValidator = captchaValidator;
        SettingProvider = settingProvider;
    }

    public virtual async Task<IdentityUserDto> RegisterAsync(RegisterDto input)
    {
        await ValidateCaptchaAsync(input, CaptchaPurpose.Register);

        if (!await SettingProvider.IsTrueAsync(IdentitySettingNames.Registration.EnableSelfRegistration))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.SelfRegistrationDisabled);
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

        string? confirmationToken = null;
        if (await SettingProvider.IsTrueAsync(IdentitySettingNames.Registration.RequireEmailConfirmation))
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
            throw new UserFriendlyException("User not found with the given email address.");
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
