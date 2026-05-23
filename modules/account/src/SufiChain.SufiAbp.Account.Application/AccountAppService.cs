using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Identity;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.EventBus.Local;

namespace SufiChain.SufiAbp.Account;

public class AccountAppService : ApplicationService, IAccountAppService
{
    protected IdentityUserManager UserManager { get; }
    protected IIdentityUserRepository UserRepository { get; }
    protected IOptions<IdentityOptions> IdentityOptions { get; }
    protected IdentityUserToIdentityUserDtoMapper UserMapper { get; }
    protected ILocalEventBus LocalEventBus { get; }

    public AccountAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IOptions<IdentityOptions> identityOptions,
        IdentityUserToIdentityUserDtoMapper userMapper,
        ILocalEventBus localEventBus)
    {
        UserManager = userManager;
        UserRepository = userRepository;
        IdentityOptions = identityOptions;
        UserMapper = userMapper;
        LocalEventBus = localEventBus;
    }

    public virtual async Task<IdentityUserDto> RegisterAsync(RegisterDto input)
    {
        var user = new IdentityUser(
            GuidGenerator.Create(),
            input.UserName,
            input.EmailAddress,
            CurrentTenant.Id
        );

        (await UserManager.CreateAsync(user, input.Password)).CheckErrors();

        await UserManager.SetEmailAsync(user, input.EmailAddress);
        await UserManager.AddDefaultRolesAsync(user);

        return UserMapper.Map(user);
    }

    public virtual async Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input)
    {
        var user = await UserManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            throw new UserFriendlyException("User not found with the given email address.");
        }
        
        var resetToken = await UserManager.GeneratePasswordResetTokenAsync(user);
        
        // Publish local event that can be handled by email module
        await LocalEventBus.PublishAsync(new PasswordResetRequestedEvent
        {
            UserId = user.Id,
            Email = input.Email,
            ResetToken = resetToken,
            AppName = input.AppName
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
}

// Event for password reset (can be handled by email module)
public class PasswordResetRequestedEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string ResetToken { get; set; }
    public string AppName { get; set; }
}
