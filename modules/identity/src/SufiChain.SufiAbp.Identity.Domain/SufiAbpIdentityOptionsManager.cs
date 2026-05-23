using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Volo.Abp.Options;

namespace SufiChain.SufiAbp.Identity;

public class SufiAbpIdentityOptionsManager : AbpDynamicOptionsManager<IdentityOptions>
{
    public SufiAbpIdentityOptionsManager(IOptionsFactory<IdentityOptions> factory)
        : base(factory)
    {
    }

    protected override Task OverrideOptionsAsync(string name, IdentityOptions options)
    {
        return Task.CompletedTask;
    }
}
