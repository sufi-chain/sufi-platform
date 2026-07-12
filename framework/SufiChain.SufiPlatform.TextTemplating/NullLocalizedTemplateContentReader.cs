namespace SufiChain.SufiPlatform.TextTemplating;

public class NullLocalizedTemplateContentReader : ILocalizedTemplateContentReader
{
    public static NullLocalizedTemplateContentReader Instance { get; } = new NullLocalizedTemplateContentReader();

    private NullLocalizedTemplateContentReader()
    {

    }

    public string? GetContentOrNull(string? culture)
    {
        return null;
    }
}
