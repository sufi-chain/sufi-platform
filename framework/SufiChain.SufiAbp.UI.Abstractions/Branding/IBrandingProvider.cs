namespace SufiChain.SufiAbp.UI.Branding;

/// <summary>
/// Provides branding information for the application.
/// </summary>
public interface IBrandingProvider
{
    /// <summary>
    /// The application name.
    /// </summary>
    string AppName { get; }

    /// <summary>
    /// URL to the logo image for light backgrounds.
    /// </summary>
    string? LogoUrl { get; }

    /// <summary>
    /// URL to the logo image for dark backgrounds.
    /// </summary>
    string? LogoReverseUrl { get; }

    /// <summary>
    /// Optional copyright text for the footer (e.g. "Copyright © SufiChain").
    /// When null, the theme may display its own default (e.g. SufiTheme uses "Copyright © SufiChain").
    /// Hosts can override by providing a custom provider or by configuring theme options.
    /// </summary>
    string? CopyrightText { get; }
}
