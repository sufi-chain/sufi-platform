using System;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Account;

public class PasswordResetDeliveryArgs : IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? AppName { get; set; }

    public string? ReturnUrl { get; set; }

    public string? ReturnUrlHash { get; set; }
}

/// <summary>
/// Builds the reset token and publishes <see cref="PasswordResetRequestedEvent"/> off the HTTP request.
/// The request itself returns on a fixed floor so a known address is not slower than an unknown one.
/// </summary>
public class PasswordResetDeliveryJob :
    AsyncBackgroundJob<PasswordResetDeliveryArgs>,
    ITransientDependency
{
    private readonly IdentityUserManager _userManager;
    private readonly ILocalEventBus _localEventBus;

    public PasswordResetDeliveryJob(IdentityUserManager userManager, ILocalEventBus localEventBus)
    {
        _userManager = userManager;
        _localEventBus = localEventBus;
    }

    public override async Task ExecuteAsync(PasswordResetDeliveryArgs args)
    {
        var user = await _userManager.FindByIdAsync(args.UserId.ToString());
        if (user == null)
        {
            return;
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _localEventBus.PublishAsync(new PasswordResetRequestedEvent
        {
            UserId = user.Id,
            Email = args.Email,
            ResetToken = resetToken,
            AppName = args.AppName,
            ReturnUrl = args.ReturnUrl,
            ReturnUrlHash = args.ReturnUrlHash
        });
    }
}
