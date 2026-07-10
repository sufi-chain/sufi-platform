using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Identity.Settings;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Identity.AspNetCore;

public class IdentityTokenOptionsConfigurator : IConfigureOptions<DataProtectionTokenProviderOptions>, ITransientDependency
{
    protected IConfiguration Configuration { get; }

    public IdentityTokenOptionsConfigurator(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void Configure(DataProtectionTokenProviderOptions options)
    {
        var emailHours = GetInt(IdentitySettingNames.Tokens.EmailConfirmationTokenLifespanHours, 24);
        options.TokenLifespan = TimeSpan.FromHours(emailHours);
    }

    protected virtual int GetInt(string name, int defaultValue)
    {
        var value = Configuration[name] ?? Configuration[$"Settings:{name}"];
        return int.TryParse(value, out var result) ? result : defaultValue;
    }
}
