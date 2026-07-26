using System;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.OpenIddict;

/// <summary>
/// Converts between OpenIddict string identifiers and <see cref="Guid"/> keys.
/// </summary>
public class OpenIddictIdentifierConverter : ITransientDependency
{
    public virtual Guid FromString(string identifier)
    {
        return string.IsNullOrEmpty(identifier) ? default : Guid.Parse(identifier);
    }

    public virtual string ToString(Guid identifier)
    {
        return identifier.ToString("D");
    }
}
