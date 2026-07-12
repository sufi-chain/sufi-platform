using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.TextTemplating;

public interface ILocalizedTemplateContentReaderFactory
{
    Task<ILocalizedTemplateContentReader> CreateAsync(TemplateDefinition templateDefinition);
}
