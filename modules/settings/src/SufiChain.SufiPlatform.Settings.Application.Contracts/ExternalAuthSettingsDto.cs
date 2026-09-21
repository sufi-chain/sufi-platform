namespace SufiChain.SufiPlatform.Settings;

public class ExternalAuthSettingsDto
{
    public ExternalAuthProviderSettingsDto Google { get; set; } = new();

    public ExternalAuthProviderSettingsDto Microsoft { get; set; } = new();

    public ExternalAuthProviderSettingsDto GitHub { get; set; } = new();
}

public class ExternalAuthProviderSettingsDto
{
    public bool Enabled { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public bool HasClientSecret { get; set; }
}

public class UpdateExternalAuthSettingsDto
{
    public ExternalAuthProviderSettingsDto Google { get; set; } = new();

    public ExternalAuthProviderSettingsDto Microsoft { get; set; } = new();

    public ExternalAuthProviderSettingsDto GitHub { get; set; } = new();
}
