using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Identity;

public class IdentitySecurityLogContext
{
    public string Identity { get; set; } = default!;

    public string Action { get; set; } = default!;

    public string UserName { get; set; } = default!;

    public string? ClientId { get; set; }

    public Dictionary<string, object> ExtraProperties { get; }

    public IdentitySecurityLogContext()
    {
        ExtraProperties = new Dictionary<string, object>();
    }

    public virtual IdentitySecurityLogContext WithProperty(string key, object value)
    {
        ExtraProperties[key] = value;
        return this;
    }
}
