namespace SufiChain.SufiAbp.TextTemplating;

public interface ILocalizedTemplateContentReader
{
    public string? GetContentOrNull(string? culture);
}
