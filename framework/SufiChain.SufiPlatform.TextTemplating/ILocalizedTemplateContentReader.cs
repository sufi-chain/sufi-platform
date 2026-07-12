namespace SufiChain.SufiPlatform.TextTemplating;

public interface ILocalizedTemplateContentReader
{
    public string? GetContentOrNull(string? culture);
}
