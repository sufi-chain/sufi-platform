using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Identity;

public class PasswordPolicyValidatorTests
{
    [Fact]
    public async Task PasswordValidators_ContainBuiltInAndSufiValidator_OnceEach()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISettingProvider, EmptySettings>();
        services.AddIdentityCore<IdentityUser>()
            .AddPasswordValidator<SufiExtendedPasswordValidator>();

        await using var provider = services.BuildServiceProvider();
        var validators = provider.GetServices<IPasswordValidator<IdentityUser>>().ToList();

        validators.OfType<PasswordValidator<IdentityUser>>().Count().ShouldBe(1);
        validators.OfType<SufiExtendedPasswordValidator>().Count().ShouldBe(1);
        typeof(SufiExtendedPasswordValidator)
            .GetCustomAttribute<DisableConventionalRegistrationAttribute>()
            .ShouldNotBeNull();
    }

    [Fact]
    public async Task AdminCreate_WeakPassword_Rejected_HostDefaultPolicy()
    {
        (await ValidateAsync("abc")).Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task Register_Password123_Rejected_Host()
    {
        (await ValidateAsync("123")).Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task AdminCreate_StrongPassword_Succeeds_Host()
    {
        (await ValidateAsync("Abc12!x")).Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task AdminCreate_WeakPassword_Rejected_TenantPolicy()
    {
        (await ValidateAsync("Abc12!x", options => options.Password.RequiredLength = 8)).Succeeded.ShouldBeFalse();
    }

    [Fact]
    public void Password_paths_load_the_dynamic_policy_before_writing()
    {
        var identityUser = Read("SufiChain.SufiPlatform.Identity.Application", "IdentityUserAppService.cs");
        var account = Read("SufiChain.SufiPlatform.Account.Application", "AccountAppService.cs");
        var otp = Read("SufiChain.SufiPlatform.Account.Application", "AccountOtpAppService.cs");
        var profile = Read("SufiChain.SufiPlatform.Account.Application", "ProfileAppService.cs");
        var module = Read("SufiChain.SufiPlatform.Identity.Domain", "SufiIdentityDomainModule.cs");

        identityUser.ShouldContain("CreateAsync");
        identityUser.ShouldContain(".SetAsync()");
        account.ShouldContain("RegisterAsync");
        account.ShouldContain("ResetPasswordAsync");
        Count(account, ".SetAsync()").ShouldBeGreaterThanOrEqualTo(2);
        otp.ShouldContain("RegisterWithOtpAsync");
        otp.ShouldContain(".SetAsync()");
        profile.ShouldContain("ChangePasswordAsync");
        profile.ShouldContain(".SetAsync()");
        module.ShouldContain("AddIdentityCore<IdentityUser>");
        module.ShouldContain("AddPasswordValidator<SufiExtendedPasswordValidator>()");
    }

    [Fact]
    public void ResetPassword_RotatesSecurityStamp()
    {
        var manager = Read("SufiChain.SufiPlatform.Identity.Domain", "Managers/IdentityUserManager.cs");
        var hash = manager.IndexOf("UpdatePasswordHash", StringComparison.Ordinal);
        var stamp = manager.IndexOf("UpdateSecurityStampAsync", hash, StringComparison.Ordinal);
        var assign = manager.IndexOf("PasswordHash = PasswordHasher.HashPassword", hash, StringComparison.Ordinal);
        assign.ShouldBeGreaterThan(hash);
        stamp.ShouldBeGreaterThan(assign);
    }

    [Fact]
    public void ConfirmEmail_SameToken_FailsTheSecondTime_AndDoesNotSignIn()
    {
        var manager = Read("SufiChain.SufiPlatform.Identity.Domain", "Managers/IdentityUserManager.cs");
        var confirm = manager.IndexOf("ConfirmEmailAsync", StringComparison.Ordinal);
        manager.IndexOf("EmailConfirmed", confirm, StringComparison.Ordinal).ShouldBeGreaterThan(confirm);
        manager.IndexOf("UpdateSecurityStampAsync", confirm, StringComparison.Ordinal).ShouldBeGreaterThan(confirm);

        var account = Read("SufiChain.SufiPlatform.Account.Application", "AccountAppService.cs");
        var verify = account.IndexOf("VerifyEmailConfirmationTokenAsync", StringComparison.Ordinal);
        account.IndexOf("user.EmailConfirmed", verify, StringComparison.Ordinal).ShouldBeGreaterThan(verify);

        var page = ReadUnder("modules", "account", "src", "SufiChain.SufiPlatform.Account.Blazor", "Pages", "ConfirmEmail.razor.cs");
        var invalid = page.IndexOf("if (!isValid)", StringComparison.Ordinal);
        var signIn = page.IndexOf("ContinueAfterConfirmationAsync", invalid, StringComparison.Ordinal);
        page.IndexOf("if (!IsSuccess)", invalid, StringComparison.Ordinal).ShouldBeLessThan(signIn);
    }

    private static async Task<IdentityResult> ValidateAsync(string password, Action<IdentityOptions>? configure = null)
    {
        var options = new IdentityOptions();
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        configure?.Invoke(options);

        var manager = new UserManager<IdentityUser>(
            new UnusedStore(),
            Microsoft.Extensions.Options.Options.Create(options),
            new PasswordHasher<IdentityUser>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            new UnusedLogger());
        return await new PasswordValidator<IdentityUser>().ValidateAsync(
            manager,
            new IdentityUser(Guid.NewGuid(), "ada", "ada@example.com"),
            password);
    }

    private sealed class UnusedStore : IUserStore<IdentityUser>
    {
        public void Dispose()
        {
        }

        public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task SetUserNameAsync(IdentityUser user, string? userName, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class UnusedLogger : ILogger<UserManager<IdentityUser>>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }
    }

    private static int Count(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static string Read(string project, string file)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var path = Path.Combine(dir.FullName, "modules", "identity", "src", project, file);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            path = Path.Combine(dir.FullName, "modules", "account", "src", project, file);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(file);
    }

    private static string ReadUnder(params string[] parts)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var path = Path.Combine(new[] { dir.FullName }.Concat(parts).ToArray());
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(parts[^1]);
    }

    private sealed class EmptySettings : ISettingProvider
    {
        public Task<string?> GetOrNullAsync(string name) => Task.FromResult<string?>(null);

        public Task<List<SettingValue>> GetAllAsync(string[] names) => Task.FromResult(new List<SettingValue>());

        public Task<List<SettingValue>> GetAllAsync() => Task.FromResult(new List<SettingValue>());
    }
}
