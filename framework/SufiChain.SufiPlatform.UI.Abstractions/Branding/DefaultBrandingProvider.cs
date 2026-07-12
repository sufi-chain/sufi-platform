namespace SufiChain.SufiPlatform.UI.Branding;

/// <summary>
/// Default implementation of IBrandingProvider.
/// Override this class to provide custom branding.
/// </summary>
public class DefaultBrandingProvider : IBrandingProvider
{
    /// <inheritdoc/>
    public virtual string AppName => "MyApplication";

    /// <inheritdoc/>
    public virtual string? LogoUrl => null;

    /// <inheritdoc/>
    public virtual string? LogoReverseUrl => null;

    /// <inheritdoc/>
    public virtual string? CopyrightText => null;
}
